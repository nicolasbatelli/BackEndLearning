'use client';

import { GoogleLogin } from '@react-oauth/google';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Container,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useAuth } from '@/context/AuthContext';
import { validateSignIn } from '@/lib/validation';

export default function SignInPage() {
  const { login, googleSignIn } = useAuth();
  const router = useRouter();
  const [usernameOrEmail, setUsernameOrEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverError, setServerError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const validationErrors = validateSignIn(usernameOrEmail, password);
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) return;

    setLoading(true);
    setServerError('');
    try {
      await login(usernameOrEmail, password);
      router.push('/');
    } catch (error) {
      setServerError(error instanceof Error ? error.message : 'Sign in failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm" sx={{ py: 8 }}>
      <Paper sx={{ p: 4 }}>
        <Typography variant="h4" gutterBottom>Sign In</Typography>
        <Typography color="text.secondary" sx={{ mb: 3 }}>Welcome back to SocialChat</Typography>
        {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}
        <Box component="form" onSubmit={handleSubmit}>
          <Stack spacing={2}>
            <TextField
              label="Username or Email"
              value={usernameOrEmail}
              onChange={(e) => setUsernameOrEmail(e.target.value)}
              error={Boolean(errors.usernameOrEmail)}
              helperText={errors.usernameOrEmail}
              fullWidth
            />
            <TextField
              label="Password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              error={Boolean(errors.password)}
              helperText={errors.password}
              fullWidth
            />
            <Button type="submit" variant="contained" disabled={loading}>
              Sign In
            </Button>
          </Stack>
        </Box>
        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center' }}>
          <GoogleLogin
            onSuccess={(response) => {
              if (!response.credential) return;
              setLoading(true);
              googleSignIn(response.credential)
                .then(() => router.push('/'))
                .catch((error) => setServerError(error instanceof Error ? error.message : 'Google sign in failed.'))
                .finally(() => setLoading(false));
            }}
            onError={() => setServerError('Google sign in failed.')}
          />
        </Box>
        <Typography sx={{ mt: 3 }}>
          Don&apos;t have an account? <Link href="/sign-up">Sign up</Link>
        </Typography>
      </Paper>
    </Container>
  );
}
