using System.Collections;
using System.Collections.Immutable;
using System.Text.Json;
using backend.Models.Domain;

public class GamesList : List<Game>, IEnumerable<Game> // cia yra paturbintas list tiesiog
{
    public GamesList(List<Game> _games) : base(_games) { }

    public IEnumerable<Game> GetCustomEnumerator() // returnina collection, which can be itterated over. naudojam default enumerator
    {
        foreach (var game in this)
        {
            var moves =  JsonSerializer.Deserialize<List<string>>(game.MovesArraySerialized);
            if (moves != null && moves.Count > 1)
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