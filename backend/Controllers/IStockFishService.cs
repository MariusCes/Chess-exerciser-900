namespace CHESSPROJ.Controllers
{
    public interface IStockfishService
    {
        void SetLevel(int level);
        string GetBestMove();
        void SetPosition(string movesMade, string move);
        bool IsMoveCorrect(string currentPosition ,string move);
        string GetFen();

        string GetEvalType();
        int GetEvalVal();
    }
}
