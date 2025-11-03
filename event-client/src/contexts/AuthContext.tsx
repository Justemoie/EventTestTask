import React, { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import { User, LoginUser, RegisterUser, UpdateUserRequest } from '../types';
import { apiService } from '../services/api';

interface AuthContextType {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
    loading: boolean;
    login: (credentials: LoginUser) => Promise<void>;
    register: (userData: RegisterUser) => Promise<void>;
    logout: () => Promise<void>;
    updateUser: (userData: UpdateUserRequest) => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};

interface AuthProviderProps {
    children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);
    const [token, setToken] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const initializeAuth = async () => {
            const storedToken = localStorage.getItem('token');
            const storedUser = localStorage.getItem('user');

            if (storedToken && storedUser) {
                setToken(storedToken);
                setUser(JSON.parse(storedUser));
            }
            setLoading(false);
        };

        initializeAuth();
    }, []);

    const login = async (credentials: LoginUser) => {
        try {
            const tokenResponse = await apiService.login(credentials);
            setToken(tokenResponse.accessToken);
            
            // Получаем информацию о пользователе
            const userData = await apiService.getUserByEmail(credentials.email);
            setUser(userData);
            
            localStorage.setItem('token', tokenResponse.accessToken);
            localStorage.setItem('user', JSON.stringify(userData));
        } catch (error) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            throw error;
        }
    };

    const register = async (userData: RegisterUser) => {
        await apiService.register(userData);
    };

    const logout = async () => {
        try {
            await apiService.logout();
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            setUser(null);
            setToken(null);
            localStorage.removeItem('token');
            localStorage.removeItem('user');
        }
    };

    const updateUser = async (userData: UpdateUserRequest): Promise<void> => {
        if (!user) return;
        
        try {
            await apiService.updateUser(user.id, userData);
            
            // Обновляем состояние пользователя
            setUser(prev => prev ? { ...prev, ...userData } : null);
            
            // Обновляем localStorage
            const storedUser = localStorage.getItem('user');
            if (storedUser) {
                const parsedUser = JSON.parse(storedUser);
                localStorage.setItem('user', JSON.stringify({ ...parsedUser, ...userData }));
            }
        } catch (error) {
            console.error('Error updating user:', error);
            throw error;
        }
    };

    const value: AuthContextType = {
        user,
        token,
        isAuthenticated: !!user,
        loading,
        login,
        register,
        logout,
        updateUser
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};