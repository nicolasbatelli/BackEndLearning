'use client';

import { useEffect, useRef, useState } from 'react';
import {
  Box,
  Button,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import StarBorderIcon from '@mui/icons-material/StarBorder';
import StarIcon from '@mui/icons-material/Star';
import { apiRequest, ConversationDto, MessageDto } from '@/lib/api';
import { createChatConnection } from '@/lib/signalr';
import { useAuth } from '@/context/AuthContext';

interface ChatWindowProps {
  conversation?: ConversationDto;
  onFavoriteChange?: (conversationId: string, isFavorite: boolean) => void;
}

export default function ChatWindow({ conversation, onFavoriteChange }: ChatWindowProps) {
  const { accessToken, user } = useAuth();
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [content, setContent] = useState('');
  const [isFavorite, setIsFavorite] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!conversation || !accessToken) {
      setMessages([]);
      return;
    }

    setIsFavorite(conversation.isFavorite);
    apiRequest<MessageDto[]>(`/api/chat/conversations/${conversation.id}/messages`, {}, accessToken)
      .then(setMessages)
      .catch(() => setMessages([]));

    const connection = createChatConnection(accessToken);
    connection.start()
      .then(() => connection.invoke('JoinConversation', conversation.id))
      .catch(() => undefined);

    connection.on('ReceiveMessage', (message: MessageDto) => {
      if (message.conversationId === conversation.id) {
        setMessages((current) => [...current, message]);
      }
    });

    return () => {
      connection.invoke('LeaveConversation', conversation.id).catch(() => undefined);
      connection.stop().catch(() => undefined);
    };
  }, [conversation, accessToken]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const sendMessage = async () => {
    if (!conversation || !accessToken || !content.trim()) return;
    await apiRequest<MessageDto>('/api/chat/messages', {
      method: 'POST',
      body: JSON.stringify({ conversationId: conversation.id, content }),
    }, accessToken);
    setContent('');
  };

  const toggleFavorite = async () => {
    if (!conversation || !accessToken) return;
    const next = !isFavorite;
    await apiRequest('/api/social/favorites', {
      method: 'POST',
      body: JSON.stringify({ conversationId: conversation.id, isFavorite: next }),
    }, accessToken);
    setIsFavorite(next);
    onFavoriteChange?.(conversation.id, next);
  };

  if (!conversation) {
    return (
      <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100vh' }}>
        <Typography color="text.secondary">Select a chat to start messaging</Typography>
      </Box>
    );
  }

  const title = conversation.name
    ?? conversation.participants.find((p) => p.userId !== user?.id)?.fullName
    ?? 'Chat';

  return (
    <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', height: '100vh' }}>
      <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h6">{title}</Typography>
        <Button
          variant={isFavorite ? 'contained' : 'outlined'}
          color={isFavorite ? 'warning' : 'inherit'}
          startIcon={isFavorite ? <StarIcon /> : <StarBorderIcon />}
          onClick={toggleFavorite}
        >
          {isFavorite ? 'Quitar de favoritos' : 'Agregar a favoritos'}
        </Button>
      </Box>
      <Box sx={{ flex: 1, overflow: 'auto', p: 2 }}>
        <Stack spacing={1}>
          {messages.map((message) => (
            <Paper
              key={message.id}
              sx={{
                p: 1.5,
                alignSelf: message.senderId === user?.id ? 'flex-end' : 'flex-start',
                maxWidth: '70%',
                bgcolor: message.senderId === user?.id ? 'primary.light' : 'grey.100',
              }}
            >
              <Typography variant="caption" sx={{ display: 'block' }}>{message.senderUsername}</Typography>
              <Typography>{message.content}</Typography>
            </Paper>
          ))}
          <div ref={bottomRef} />
        </Stack>
      </Box>
      <Box sx={{ p: 2, display: 'flex', gap: 1 }}>
        <TextField
          fullWidth
          placeholder="Type a message"
          value={content}
          onChange={(e) => setContent(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              e.preventDefault();
              sendMessage().catch(() => undefined);
            }
          }}
        />
        <Button variant="contained" onClick={() => sendMessage()}>Send</Button>
      </Box>
    </Box>
  );
}
