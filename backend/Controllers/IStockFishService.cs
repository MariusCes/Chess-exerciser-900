namespace CHESSPROJ.Controllers
{
    public interface IStockfishService
    {
        void SetLevel(int level);
        string GetBestMove();
        void SetPosition(params string[] moves);
        bool IsMoveCorrect(string move);
        string GetFenPosition();
    }
}
