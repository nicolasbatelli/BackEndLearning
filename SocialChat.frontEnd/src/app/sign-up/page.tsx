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
import { SignUpFormValues, validateSignUp } from '@/lib/validation';

const initialValues: SignUpFormValues = {
  username: '',
  password: '',
  firstName: '',
  middleName: '',
  lastName: '',
  email: '',
};

export default function SignUpPage() {
  const { register, googleSignIn, uploadAvatar, login } = useAuth();
  const router = useRouter();
  const [values, setValues] = useState<SignUpFormValues>(initialValues);
  const [avatar, setAvatar] = useState<File | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverError, setServerError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleChange = (field: keyof SignUpFormValues) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setValues((current) => ({ ...current, [field]: event.target.value }));
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const validationErrors = validateSignUp(values);
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) return;

    setLoading(true);
    setServerError('');
    try {
      await register({
        username: values.username,
        password: values.password,
        firstName: values.firstName,
        middleName: values.middleName || undefined,
        lastName: values.lastName,
        email: values.email,
      });

      if (avatar) {
        try {
          await login(values.username, values.password);
          await uploadAvatar(avatar);
        } catch {
          // Avatar upload requires verified email; user can upload after verification.
        }
      }

      setSuccess('Registration successful. Please check your email to verify your account.');
    } catch (error) {
      setServerError(error instanceof Error ? error.message : 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="md" sx={{ py: 6 }}>
      <Paper sx={{ p: 4 }}>
        <Typography variant="h4" gutterBottom>Create Account</Typography>
        <Typography color="text.secondary" sx={{ mb: 3 }}>Standalone sign up for SocialChat</Typography>
        {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}
        {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}
        <Box component="form" onSubmit={handleSubmit}>
          <Stack spacing={2}>
            <TextField label="Username" value={values.username} onChange={handleChange('username')} error={Boolean(errors.username)} helperText={errors.username} fullWidth />
            <TextField label="Password" type="password" value={values.password} onChange={handleChange('password')} error={Boolean(errors.password)} helperText={errors.password} fullWidth />
            <TextField label="First Name" value={values.firstName} onChange={handleChange('firstName')} error={Boolean(errors.firstName)} helperText={errors.firstName} fullWidth />
            <TextField label="Middle Name" value={values.middleName} onChange={handleChange('middleName')} error={Boolean(errors.middleName)} helperText={errors.middleName} fullWidth />
            <TextField label="Last Name" value={values.lastName} onChange={handleChange('lastName')} error={Boolean(errors.lastName)} helperText={errors.lastName} fullWidth />
            <TextField label="Email" type="email" value={values.email} onChange={handleChange('email')} error={Boolean(errors.email)} helperText={errors.email} fullWidth />
            <Button variant="outlined" component="label">
              Upload Profile Picture
              <input hidden type="file" accept="image/*" onChange={(e) => setAvatar(e.target.files?.[0] ?? null)} />
            </Button>
            {avatar && <Typography variant="body2">Selected: {avatar.name}</Typography>}
            <Button type="submit" variant="contained" disabled={loading}>Sign Up</Button>
          </Stack>
        </Box>
        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center' }}>
          <GoogleLogin
            onSuccess={(response) => {
              if (!response.credential) return;
              googleSignIn(response.credential).then(() => router.push('/')).catch((error) => {
                setServerError(error instanceof Error ? error.message : 'Google sign in failed.');
              });
            }}
            onError={() => setServerError('Google sign in failed.')}
          />
        </Box>
        <Typography sx={{ mt: 3 }}>
          Already have an account? <Link href="/sign-in">Sign in</Link>
        </Typography>
      </Paper>
    </Container>
  );
}
