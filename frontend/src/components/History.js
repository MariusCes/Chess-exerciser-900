import React, { useState, useEffect } from "react";
import "../styles/History.css";
import { useAuth } from './AuthContext';

const History = () => {
    const { token } = useAuth();

    const [games, setGames] = useState([]);
    const [expandedGameId, setExpandedGameId] = useState(null);
    const [gameMoves, setGameMoves] = useState({});
    const [loadingStates, setLoadingStates] = useState({});

    const getGames = async () => {
        const response = await fetch(
            "http://localhost:5030/api/chess/games",
            {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
            }
        );
        const data = await response.json();
        setGames(data);
    };

    useEffect(() => {
        getGames();
    }, []);

    const toggleGameDetails = async (id, serializedMoves) => {
        // If the clicked game is already expanded, collapse it
        setExpandedGameId(expandedGameId === id ? null : id);

        // Load moves if not already loaded
        if (!gameMoves[id]) {
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
                    const isExpanded = expandedGameId === game.gameId;

                    return (
                        <div
                            key={game.gameId}
                            className={`game-item ${game.wld === 1
                                ? "game-won"
                                : game.wld === 0
                                    ? "game-draw"
                                    : "game-lost"
                                }`}
                            onClick={() => toggleGameDetails(game.gameId, game.movesArraySerialized)}
                        >
                            <div className="game-summary">
                                <span
                                    className={`game-result ${game.wld === 1
                                        ? "game-won"
                                        : game.wld === 0
                                            ? "game-draw"
                                            : "game-lost"
                                        }`}
                                >
                                    {game.wld === 1
                                        ? "Victory"
                                        : game.wld === 0
                                            ? "Draw"
                                            : "Defeat"}
                                </span>
                            </div>

                            {/* Show details only for the expanded game */}
                            {isExpanded && (
                                <div className="game-details">
                                    <h4 className="details-header">Move History</h4>
                                    <div className="move-list-container">
                                        {loadingStates[game.gameId] ? (
                                            <li>Loading moves...</li>
                                        ) : game.movesArraySerialized === null ? (
                                            <div className="no-moves">Game has 0 moves!</div>
                                        ) : (
                                            <ul className="move-list">
                                                {(gameMoves[game.gameId] || []).map((move, index) => (
                                                    <li
                                                        key={index}
                                                        className={`move-item ${index % 2 === 0 ? "your-move" : "bot-move"}`}
                                                    >
                                                        {move}
                                                    </li>
                                                ))}
                                            </ul>
                                        )}
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