import { render, screen } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import AppProviders from '@/components/AppProviders';

jest.mock('@/context/AuthContext', () => ({
  AuthProvider: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  useAuth: () => ({
    user: null,
    accessToken: null,
    roles: [],
    isAuthenticated: false,
    isLoading: false,
    login: jest.fn(),
    register: jest.fn(),
    googleSignIn: jest.fn(),
    logout: jest.fn(),
    refreshSession: jest.fn(),
    uploadAvatar: jest.fn(),
  }),
}));

describe('AppProviders', () => {
  it('renders children', () => {
    render(
      <AppProviders>
        <div>SocialChat</div>
      </AppProviders>,
    );

    expect(screen.getByText('SocialChat')).toBeInTheDocument();
  });
});
