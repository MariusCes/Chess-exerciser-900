import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import '../styles/Login.css';

const Register = () => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);

    const handleRegister = async (e) => {
        e.preventDefault();


        if (password !== confirmPassword) {
            alert("Passwords do not match!");
            return;
        }

        const response = await fetch(
            "http://localhost:5030/api/chess/register",
            {
                method: "POST",
                body: JSON.stringify({
                    UserName: username,
                    Email: email,
                    Password: password,
                    ConfirmPassword: confirmPassword
                }),
                headers: {
                    "Content-type": "application/json",
                },
            }
        );

        const data = await response.json();

        if (data.message !== null) {
            alert("register successful! go log-in!");
        } else {
            alert("Something is wrong. Try again!");
        }
    };

    return (
        <div className="login-container">
            <div className="login-card">
                <h2 className="login-title">JOIN US, <br /> FUTURE CHESS MASTER</h2>
                <form onSubmit={handleRegister}>
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
                            type="email"
                            className="form-control login-input"
                            placeholder="Email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
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
                    </div>
                    <div className="mb-3">
                        <input
                            type={showPassword ? "text" : "password"}
                            className="form-control login-input"
                            placeholder="Confirm Password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                        <div className="form-check mt-2">
                            <input
                                type="checkbox"
                                className="form-check-input"
                                id="showPassword"
                                checked={showPassword}
                                onChange={() => setShowPassword(!showPassword)}
                            />
                        </div>
                    </div>
                    <button
                        type="submit"
                        className="btn w-100 mb-3 login-btn"
                    >
                        Register
                    </button>
                    <div className="text-center">
                        <Link
                            to="/login"
                            className="login-link"
                        >
                            Already have an account? Login
                        </Link>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default Register;