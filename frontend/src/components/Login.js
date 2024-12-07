import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import '../styles/Login.css';

const Login = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const navigate = useNavigate(); // Allows to use the navigate(directory) command

    const handleLogin = async (e) => {
        e.preventDefault();
        
        if (username === "testuser" && password === "password123") {
            localStorage.setItem("isLoggedIn", "true");
            localStorage.setItem("username", username);
            alert("Login successful!");
            navigate("/play"); // Go to play screen after successful login
        } else {
            alert("Invalid credentials. Please try again.");
        }
    };

    return (
        <div className="login-container">
            <div className="login-card">
                <h2 className="login-title">WELCOME BACK, <br /> CHESS MASTER</h2>
                <form onSubmit={handleLogin}>
                    <div className="mb-3">
                        <input 
                            type="text" 
                            className="form-control login-input" 
                            placeholder="Username"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                        />
                    </div>
                    <div className="mb-3">
                        <input 
                            type={showPassword ? "text" : "password"} 
                            className="form-control login-input" 
                            placeholder="Password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                        <div className="form-check mt-2">
                            <input 
                                type="checkbox" 
                                className="form-check-input" 
                                id="showPassword" 
                                checked={showPassword}
                                onChange={() => setShowPassword(!showPassword)} 
                            />
                            <label className="form-check-label" htmlFor="showPassword">Show Password</label>
                        </div>
                    </div>
                    <button 
                        type="submit" 
                        className="btn w-100 mb-3 login-btn"
                    >
                        Login
                    </button>
                    <div className="text-center">
                        <Link 
                            to="/register" 
                            className="login-link"
                        >
                            Create an Account
                        </Link>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default Login;