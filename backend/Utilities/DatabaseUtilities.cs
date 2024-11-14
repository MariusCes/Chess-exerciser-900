using Stockfish.NET;

namespace CHESSPROJ.DatabaseUtilities
{
    public class DatabaseUtilities 
    {
        private readonly ChessDbContext dbContext;

        public DatabaseUtilities(ChessDbContext dbContext) 
        {
            this.dbContext = dbContext;
        }


        public bool AddGame(Game game) 
        {
            return false;
            // check if this game already exists

            // code to save changes. returns: T=> all good. F => all bad
        }

        public Game GetGameById(int id) 
        {
            return null;
        }

        public bool updateGame(Game game) 
        { 
            return true;        // update the game in DB. (find existing and update it)
        }


        public List<Game> GetGamesList()
        {
            // Retrieve all games as a List<Game>
            List<Game> gamesList;// = dbContext.Games.ToList();
            return gamesList;
        }
    }
}