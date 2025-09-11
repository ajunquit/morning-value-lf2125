using Morning.Value.Application.Common.Dtos;
using Morning.Value.Application.Loans.Dtos;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Exceptions;
using Morning.Value.Domain.Loans.Entity;
using Morning.Value.Domain.Loans.Enums;

namespace Morning.Value.Application.Loans.Services
{
    public class LoanAppService : ILoanAppService
    {
        private readonly IUnitOfWorkAsync _uow;

        public LoanAppService(IUnitOfWorkAsync uow)
        {
            _uow = uow;
        }

        /// <summary>
        /// Crea un préstamo si hay disponibilidad. Evita duplicados activos del mismo libro para el mismo usuario.
        /// </summary>
        public async Task<BorrowResponse> BorrowAsync(Guid userId, Guid bookId, CancellationToken ct = default)
        {
            // 1) Cargar libro
            var book = await _uow.BookRepository.GetAsync(bookId);
            if (book is null) throw new DomainException("Libro no encontrado.");

            // 2) Validar disponibilidad
            if (!book.HasAvailability())
                throw new DomainException("No hay copias disponibles.");

            // 3) Evitar préstamo duplicado activo del mismo libro
            var active = await _uow.LoanRepository.GetActiveByUserAndBookAsync(userId, bookId, ct);
            if (active is not null)
                throw new DomainException("Ya tienes un préstamo activo de este libro.");

            // 4) Reservar (decrementa)
            book.ReserveOne();
            await _uow.BookRepository.UpdateAsync(book);

            // 5) Crear préstamo
            var loan = Loan.Create(userId: userId, bookId: bookId);
            await _uow.LoanRepository.InsertAsync(loan);

            // 6) Commit
            await _uow.SaveChangesAsync(ct);

            return new BorrowResponse
            {
                LoanId = loan.Id,
                BookId = bookId,
                RemainingCopies = book.AvailableCopies,
                LoanDateUtc = loan.LoanDateUtc
            };
        }

        public async Task<PagedResult<LoanHistoryItemResponse>> GetHistoryByUserAsync(Guid userId, string? query, LoanStatus? status, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, total) = await _uow.LoanRepository.GetHistoryByUserAsync(userId, query, status, page, pageSize, ct);

            var dtos = items.Select(x => new LoanHistoryItemResponse
            {
                LoanId = x.LoanId,
                BookTitle = x.BookTitle,
                Author = x.Author,
                Genre = x.Genre,
                LoanDate = x.LoanDateUtc,
                ReturnDate = x.ReturnDateUtc,
                Status = x.ReturnDateUtc.HasValue ? LoanStatus.Returned : LoanStatus.Borrowed
            }).ToList();

            return new PagedResult<LoanHistoryItemResponse>
            {
                Items = dtos,
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = total,
                Query = query,
                StatusFilter = status?.ToString()?.ToLower()
            };
        }

        /// <summary>
        /// Marca devolución y devuelve una copia al inventario.
        /// </summary>
        public async Task<bool> ReturnAsync(Guid loanId, CancellationToken ct = default)
        {
            // 1) Cargar préstamo
            var loan = await _uow.LoanRepository.GetAsync(loanId);
            if (loan is null) return false;                // o lanzar excepción si prefieres

            if (loan.ReturnDateUtc is not null) return false; // ya devuelto

            // 2) Cargar libro
            var book = await _uow.BookRepository.GetAsync(loan.BookId);
            if (book is null) throw new DomainException("Libro del préstamo no encontrado.");

            // 3) Marcar devolución + incrementar disponibilidad
            loan.MarkReturned();
            book.ReturnOne();

            await _uow.LoanRepository.UpdateAsync(loan);
            await _uow.BookRepository.UpdateAsync(book);

            // 4) Commit
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}
