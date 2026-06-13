'use client';

import { jwtDecode } from 'jwt-decode';
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import {
  apiRequest,
  AuthResponse,
  UserDto,
} from '@/lib/api';

interface JwtPayload {
  sub: string;
  unique_name: string;
  email: string;
  role?: string | string[];
  full_name?: string;
  exp: number;
}

const AUTH_STORAGE_KEY = 'socialchat.auth';

interface AuthContextValue {
  user: UserDto | null;
  accessToken: string | null;
  roles: string[];
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (usernameOrEmail: string, password: string) => Promise<void>;
  register: (payload: {
    username: string;
    password: string;
    firstName: string;
    middleName?: string;
    lastName: string;
    email: string;
  }) => Promise<void>;
  googleSignIn: (idToken: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshSession: () => Promise<void>;
  uploadAvatar: (file: File) => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function extractRoles(payload: JwtPayload): string[] {
  if (!payload.role) return [];
  return Array.isArray(payload.role) ? payload.role : [payload.role];
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [roles, setRoles] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const applyAuth = useCallback((response: AuthResponse) => {
    const payload = jwtDecode<JwtPayload>(response.accessToken);
    setAccessToken(response.accessToken);
    setUser(response.user);
    setRoles(extractRoles(payload));
  }, []);

  const clearAuth = useCallback(() => {
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
    setUser(null);
    setAccessToken(null);
    setRoles([]);
  }, []);

  const persistAuth = useCallback((response: AuthResponse) => {
    window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(response));
    applyAuth(response);
  }, [applyAuth]);

  const refreshSession = useCallback(async () => {
    const response = await apiRequest<AuthResponse>('/api/auth/refresh', { method: 'POST' });
    persistAuth(response);
  }, [persistAuth]);

  useEffect(() => {
    const storedAuth = window.localStorage.getItem(AUTH_STORAGE_KEY);
    if (storedAuth) {
      try {
        const parsedAuth = JSON.parse(storedAuth) as AuthResponse;
        const payload = jwtDecode<JwtPayload>(parsedAuth.accessToken);
        if (payload.exp * 1000 > Date.now()) {
          applyAuth(parsedAuth);
        } else {
          window.localStorage.removeItem(AUTH_STORAGE_KEY);
        }
      } catch {
        window.localStorage.removeItem(AUTH_STORAGE_KEY);
      }
    }

    refreshSession()
      .catch(() => undefined)
      .finally(() => setIsLoading(false));
  }, [applyAuth, refreshSession]);

  useEffect(() => {
    if (!accessToken) return;
    const payload = jwtDecode<JwtPayload>(accessToken);
    const expiresInMs = payload.exp * 1000 - Date.now() - 60_000;
    if (expiresInMs <= 0) {
      refreshSession().catch(() => undefined);
      return;
    }
    const timer = window.setTimeout(() => {
      refreshSession().catch(() => undefined);
    }, expiresInMs);
    return () => window.clearTimeout(timer);
  }, [accessToken, refreshSession]);

  const login = useCallback(async (usernameOrEmail: string, password: string) => {
    const response = await apiRequest<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ usernameOrEmail, password }),
    });
    persistAuth(response);
  }, [persistAuth]);

  const register = useCallback(async (payload: {
    username: string;
    password: string;
    firstName: string;
    middleName?: string;
    lastName: string;
    email: string;
  }) => {
    await apiRequest('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  }, []);

  const googleSignIn = useCallback(async (idToken: string) => {
    const response = await apiRequest<AuthResponse>('/api/auth/google', {
      method: 'POST',
      body: JSON.stringify({ idToken }),
    });
    persistAuth(response);
  }, [persistAuth]);

  const logout = useCallback(async () => {
    if (accessToken) {
      await apiRequest('/api/auth/logout', { method: 'POST' }, accessToken).catch(() => undefined);
    }
    clearAuth();
  }, [accessToken, clearAuth]);

  const uploadAvatar = useCallback(async (file: File) => {
    if (!accessToken) throw new Error('Not authenticated');
    const formData = new FormData();
    formData.append('file', file);
    const updatedUser = await apiRequest<UserDto>('/api/auth/avatar', {
      method: 'POST',
      body: formData,
    }, accessToken);
    setUser(updatedUser);
  }, [accessToken]);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    accessToken,
    roles,
    isAuthenticated: Boolean(user && accessToken),
    isLoading,
    login,
    register,
    googleSignIn,
    logout,
    refreshSession,
    uploadAvatar,
  }), [user, accessToken, roles, isLoading, login, register, googleSignIn, logout, refreshSession, uploadAvatar]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
