import { Link, useNavigate } from 'react-router-dom';
import '../styles/Navbar.css';

const Navbar = () => {
    const navigate = useNavigate();

    const isLoggedIn = sessionStorage.getItem("isLoggedIn") === "true";
    const username = sessionStorage.getItem("username");

    const handleLogout = () => {
        sessionStorage.removeItem("isLoggedIn");
        sessionStorage.removeItem("username");
        sessionStorage.removeItem("token");
        navigate("/login");
    };

    return (
        <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
            <div className="container-fluid">
                <div className="navbar-left">
                    {isLoggedIn ? (
                        <>
                            <span className="navbar-text username-cool">
                                Welcome, Chess Master!
                            </span>
                            <button
                                className="btn btn-link nav-link logout-button"
                                onClick={handleLogout}
                            >
                                Logout
                            </button>
                        </>
                    ) : (
                        <Link className="nav-link login-link" to="/login">
                            Login
                        </Link>
                    )}
                </div>

                <Link className="navbar-brand chess-game-title" to="/">
                    CHESS EXERCISER
                </Link>

                <div className="navbar-right">
                    <ul className="navbar-nav">
                        <li className="nav-item">
                            <Link className="nav-link" to="/home">Home</Link>
                        </li>
                        <li className="nav-item">
                            <Link className="nav-link" to="/play">Play</Link>
                        </li>
                        <li className="nav-item">
                            <Link className="nav-link" to="/history">History</Link>
                        </li>
                        <li className="nav-item">
                            <Link className="nav-link" to="/about">About</Link>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>
    );
};

export default Navbar;
