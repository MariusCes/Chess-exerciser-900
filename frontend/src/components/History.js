import React, { useState, useEffect } from "react";
import "../styles/History.css";

const History = () => {
    const [games, setGames] = useState([]);
    const [expandedGame, setExpandedGame] = useState(null); // Tracks the expanded game ID
    const [gameMoves, setGameMoves] = useState({}); // Stores move lists for each game ID
    const [loadingStates, setLoadingStates] = useState({}); // Tracks loading state for each game ID

    const getGames = async () => {
        const response = await fetch("http://localhost:5030/api/chess/7097194a-84a3-4010-9bf8-028f4869c54f/games");
        const data = await response.json();
        setGames(data);
    };

    useEffect(() => {
        getGames(); // Fetch games when the component mounts
    }, []);

    const toggleGameDetails = async (id, serializedMoves) => {
        if (expandedGame === id) {
            // Collapse if the same game is clicked again
            setExpandedGame(null);
            return;
        }

        setExpandedGame(id); // Expand the selected game

        if (!gameMoves[id]) {
            // If moves for this game are not already loaded, fetch and store them
            setLoadingStates((prev) => ({ ...prev, [id]: true }));
            const moves = JSON.parse(serializedMoves);
            setGameMoves((prev) => ({ ...prev, [id]: moves }));
            setLoadingStates((prev) => ({ ...prev, [id]: false }));
        }
    };

    return (
        <div className="history-container mt-3">
            <h1 className="history-title">Game History</h1>
            <div className="game-list">
                {games.map((game) => {
                    const isExpanded = expandedGame === game.id;

                    return (
                        <div
                            key={game.id}
                            className={`game-item ${game.wld === 1
                                ? "game-won"
                                : game.wld === 0
                                    ? "game-lost"
                                    : "game-draw"
                                }`}

                            onClick={() => toggleGameDetails(game.id, game.movesArraySerialized)}
                        >
                            <div className="game-summary">
                                <span
                                    className={`game-item ${game.wld === 1
                                        ? "game-won"
                                        : game.wld === 0
                                            ? "game-lost"
                                            : "game-draw"
                                        }`}
                                >
                                    {game.wld === 1
                                        ? "Victory"
                                        : game.wld === 0
                                            ? "Defeat"
                                            : "Draw"}
                                </span>
                            </div>

                            {/* Show details only for the expanded game */}
                            {isExpanded && (
                                <div className="game-details">
                                    <h4 className="details-header">Move History</h4>
                                    <div className="move-list-container">
                                        <ul className="move-list">
                                            {loadingStates[game.id] ? (
                                                <li>Loading moves...</li>
                                            ) : (
                                                (gameMoves[game.id] || []).map((move, index) => (
                                                    <li
                                                        key={index}
                                                        className={`move-item ${index % 2 === 0 ? "your-move" : "bot-move"}`}
                                                    >
                                                        {move}
                                                    </li>
                                                ))
                                            )}
                                        </ul>
                                    </div>
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

export default History;
