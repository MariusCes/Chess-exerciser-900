using System.Collections;
using System.Collections.Immutable;
using backend.Models.Domain;

public class GamesList : List<Game>, IEnumerable<Game> // cia yra paturbintas list tiesiog
{
    public GamesList(List<Game> _games) : base(_games) { }

    public IEnumerable<Game> GetCustomEnumerator() // returnina collection, which can be itterated over. naudojam default enumerator
    {
        foreach (var game in this)
        {
            if (game.MovesArray != null && game.MovesArray.Count > 1)
            {
                yield return game; // yield sugeneruoja values tik tada kai reikia. po truputi viska daro. lazy evaluation
            }
        }
    }

    // Override IEnumerable<Game>.GetEnumerator to use the default List<Game> enumerator
    IEnumerator<Game> IEnumerable<Game>.GetEnumerator()
    {
        return base.GetEnumerator();
    }

    // Override IEnumerable.GetEnumerator to use the default List<Game> enumerator
    IEnumerator IEnumerable.GetEnumerator()
    {
        return base.GetEnumerator();
    }
}