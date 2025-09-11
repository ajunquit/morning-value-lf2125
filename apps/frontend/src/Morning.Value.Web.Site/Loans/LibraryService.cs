

namespace Morning.Value.Web.Site.Loans
{
    public class LibraryService
    {
        public async Task<bool> BorrowAsync(int userId, int bookId)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> ReturnAsync(int loanId)
        {
            return await Task.FromResult(true);
        }
    }
}
