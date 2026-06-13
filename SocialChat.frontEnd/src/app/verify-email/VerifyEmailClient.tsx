'use client';

import { useSearchParams } from 'next/navigation';
import { useEffect, useState } from 'react';
import { Alert, Box, CircularProgress, Container, Paper, Typography } from '@mui/material';
import { apiRequest } from '@/lib/api';

export default function VerifyEmailPage() {
  const searchParams = useSearchParams();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [message, setMessage] = useState('');

  useEffect(() => {
    const token = searchParams.get('token');
    const email = searchParams.get('email');
    if (!token || !email) {
      setStatus('error');
      setMessage('Verification link is invalid.');
      return;
    }

    apiRequest('/api/auth/verify-email', {
      method: 'POST',
      body: JSON.stringify({ token, email }),
    })
      .then(() => {
        setStatus('success');
        setMessage('Your email has been verified. You can now sign in.');
      })
      .catch((error) => {
        setStatus('error');
        setMessage(error instanceof Error ? error.message : 'Verification failed.');
      });
  }, [searchParams]);

  return (
    <Container maxWidth="sm" sx={{ py: 8 }}>
      <Paper sx={{ p: 4 }}>
        <Typography variant="h4" gutterBottom>Verify Email</Typography>
        {status === 'loading' && (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        )}
        {status === 'success' && <Alert severity="success">{message}</Alert>}
        {status === 'error' && <Alert severity="error">{message}</Alert>}
      </Paper>
    </Container>
  );
}
