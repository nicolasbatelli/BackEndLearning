'use client';

import { useEffect, useState } from 'react';
import { Box, Button } from '@mui/material';
import ProtectedRoute from '@/components/ProtectedRoute';
import Sidebar from '@/components/Sidebar';
import ChatWindow from '@/components/ChatWindow';
import { apiRequest, ConversationDto, NotificationDto } from '@/lib/api';
import { createNotificationConnection } from '@/lib/signalr';
import { useAuth } from '@/context/AuthContext';

function HomePageContent() {
  const { accessToken, logout } = useAuth();
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [selectedConversationId, setSelectedConversationId] = useState<string>();
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);

  const loadConversations = () => {
    if (!accessToken) return;
    apiRequest<ConversationDto[]>('/api/chat/conversations', {}, accessToken)
      .then((data) => {
        setConversations(data);
        if (!selectedConversationId && data.length > 0) {
          setSelectedConversationId(data[0].id);
        }
      })
      .catch(() => setConversations([]));
  };

  const updateConversationFavorite = (conversationId: string, isFavorite: boolean) => {
    setConversations((current) =>
      current.map((conversation) =>
        conversation.id === conversationId
          ? { ...conversation, isFavorite }
          : conversation,
      ),
    );
  };

  useEffect(() => {
    loadConversations();
  }, [accessToken]);

  useEffect(() => {
    if (!accessToken) return;
    const connection = createNotificationConnection(accessToken);
    connection.start().catch(() => undefined);
    connection.on('ReceiveNotification', (notification: NotificationDto) => {
      setNotifications((current) => [notification, ...current]);
    });
    return () => {
      connection.stop().catch(() => undefined);
    };
  }, [accessToken]);

  const selectedConversation = conversations.find((c) => c.id === selectedConversationId);

  return (
    <Box sx={{ display: 'flex', height: '100vh' }}>
      <Sidebar
        conversations={conversations}
        selectedConversationId={selectedConversationId}
        onSelectConversation={setSelectedConversationId}
        onNotificationsChange={setNotifications}
        onConversationsRefresh={loadConversations}
      />
      <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', p: 1 }}>
          <Button size="small" onClick={() => logout()}>Logout</Button>
        </Box>
        <ChatWindow conversation={selectedConversation} onFavoriteChange={updateConversationFavorite} />
      </Box>
    </Box>
  );
}

export default function HomePage() {
  return (
    <ProtectedRoute>
      <HomePageContent />
    </ProtectedRoute>
  );
}
