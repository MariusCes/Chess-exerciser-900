import React, { useState, useEffect } from "react";
import "../styles/History.css";

const mockGames = [
    {
        id: 1,
        outcome: "won",
        duration: "12:34",
        moves: ["e4", "e5", "Nf3", "Nc6", "Bb5", "a6", "Ba4", "Nf6", "O-O"],
    },
    {
        id: 2,
        outcome: "lost",
        duration: "08:15",
        moves: ["d4", "d5", "c4", "e6", "Nc3", "Nf6", "Bg5", "Bb4", "e3"],
    },
    {
        id: 3,
        outcome: "won",
        duration: "15:45",
        moves: ["g3", "d5", "Bg2", "e6", "d3", "c5", "Nf3", "Nc6", "O-O"],
    },
];

const History = () => {
    const [games, setGames] = useState([]);
    const [expandedGame, setExpandedGame] = useState(null);

    useEffect(() => {
        setGames(mockGames);
    }, []);

    const toggleGameDetails = (id) => {
        setExpandedGame(expandedGame === id ? null : id);
    };

    return (
        <div className="history-container mt-3">
            <h1 className="history-title">Game History</h1>
            <div className="game-list">
                {mockGames.map((game) => (
                    <div
                        key={game.id}
                        className={`game-item ${game.outcome === "won" ? "game-won" : "game-lost"}`}
                        onClick={() => toggleGameDetails(game.id)}
                    >
                        <div className="game-summary">
                            <span className="game-outcome">
                                {game.outcome === "won" ? "Victory" : "Defeat"}
                            </span>
                            <span className="game-duration">{game.duration}</span>
                        </div>
                        {expandedGame === game.id && (
                            <div className="game-details">
                                <h4 className="details-header">Move History</h4>
                                <div className="move-list-container">
                                    <ul className="move-list">
                                        {game.moves.map((move, index) => (
                                            <li
                                                key={index}
                                                className={`move-item ${index % 2 === 0 ? "your-move" : "bot-move"}`}
                                            >
                                                {move}
                                            </li>
                                        ))}
                                    </ul>
                                </div>
                            </div>
                        )}
                    </div>
                ))}
            </div>
        </div>
    );
};

export default History;
