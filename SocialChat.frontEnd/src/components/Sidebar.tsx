'use client';

import {
  Avatar,
  Badge,
  Box,
  Button,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Menu,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import NotificationsIcon from '@mui/icons-material/Notifications';
import SearchIcon from '@mui/icons-material/Search';
import StarIcon from '@mui/icons-material/Star';
import GroupIcon from '@mui/icons-material/Group';
import PersonIcon from '@mui/icons-material/Person';
import PeopleIcon from '@mui/icons-material/People';
import ChatBubbleOutlinedIcon from '@mui/icons-material/ChatBubbleOutlined';
import { useCallback, useEffect, useState } from 'react';
import { apiRequest, ConversationDto, FriendDto, FriendshipDto, NotificationDto, UserSearchResultDto } from '@/lib/api';
import { useAuth } from '@/context/AuthContext';

interface SidebarProps {
  conversations: ConversationDto[];
  selectedConversationId?: string;
  onSelectConversation: (conversationId: string) => void;
  onNotificationsChange?: (notifications: NotificationDto[]) => void;
  onConversationsRefresh?: () => void;
}

export default function Sidebar({
  conversations,
  selectedConversationId,
  onSelectConversation,
  onNotificationsChange,
  onConversationsRefresh,
}: SidebarProps) {
  const { accessToken, user } = useAuth();
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<UserSearchResultDto[]>([]);
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [friends, setFriends] = useState<FriendDto[]>([]);
  const [actionableInviteIds, setActionableInviteIds] = useState<Set<string>>(() => new Set());
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [respondingId, setRespondingId] = useState<string | null>(null);

  const loadNotifications = useCallback(async () => {
    if (!accessToken) return;
    try {
      const data = await apiRequest<NotificationDto[]>('/api/social/notifications', {}, accessToken);
      setNotifications(data);
      setActionableInviteIds((current) => {
        const next = new Set(current);
        data
          .filter((notification) => notification.type === 'FriendInvite' && !notification.isRead && notification.relatedEntityId)
          .forEach((notification) => next.add(notification.id));
        return next;
      });
      onNotificationsChange?.(data);
    } catch {
      // ignore
    }
  }, [accessToken, onNotificationsChange]);

  const loadFriends = useCallback(async () => {
    if (!accessToken) return;
    try {
      const data = await apiRequest<FriendDto[]>('/api/social/friends', {}, accessToken);
      setFriends(data);
    } catch {
      // ignore
    }
  }, [accessToken]);

  useEffect(() => {
    loadNotifications();
    loadFriends();
  }, [loadNotifications, loadFriends]);

  const favorites = conversations.filter((c) => c.isFavorite);
  const groups = conversations.filter((c) => c.type === 'Group');
  const selfChat = conversations.find((c) => c.type === 'Self');
  const personalChats = conversations.filter((c) => c.type === 'Direct');

  const handleSearch = async () => {
    if (!accessToken || !searchQuery.trim()) return;
    const results = await apiRequest<UserSearchResultDto[]>(
      `/api/social/users/search?query=${encodeURIComponent(searchQuery)}`,
      {},
      accessToken,
    );
    setSearchResults(results);
  };

  const sendInvite = async (addresseeId: string) => {
    if (!accessToken) return;
    await apiRequest('/api/social/friends/invite', {
      method: 'POST',
      body: JSON.stringify({ addresseeId }),
    }, accessToken);
    await handleSearch();
  };

  const markNotificationRead = async (notificationId: string) => {
    if (!accessToken) return;
    await apiRequest(`/api/social/notifications/${notificationId}/read`, {
      method: 'POST',
    }, accessToken).catch(() => undefined);
  };

  const handleNotificationsOpen = async (element: HTMLElement) => {
    setAnchorEl(element);

    const unreadNotifications = notifications.filter((notification) => !notification.isRead);
    if (!accessToken || unreadNotifications.length === 0) return;

    const updatedNotifications = notifications.map((notification) => ({ ...notification, isRead: true }));
    setNotifications(updatedNotifications);
    onNotificationsChange?.(updatedNotifications);

    await Promise.all(unreadNotifications.map((notification) => markNotificationRead(notification.id)));
  };

  const respondToInvite = async (notification: NotificationDto, accept: boolean) => {
    if (!accessToken || !notification.relatedEntityId) return;
    setRespondingId(notification.id);
    try {
      const friendship = await apiRequest<FriendshipDto>('/api/social/friends/respond', {
        method: 'POST',
        body: JSON.stringify({ friendshipId: notification.relatedEntityId, accept }),
      }, accessToken);

      setActionableInviteIds((current) => {
        const next = new Set(current);
        next.delete(notification.id);
        return next;
      });

      if (accept) {
        await apiRequest<ConversationDto>('/api/chat/conversations', {
          method: 'POST',
          body: JSON.stringify({ type: 'Direct', participantIds: [friendship.requesterId] }),
        }, accessToken).catch(() => undefined);
        onConversationsRefresh?.();
      }

      await markNotificationRead(notification.id);
      await loadNotifications();
      if (accept) {
        await loadFriends();
      }
    } finally {
      setRespondingId(null);
    }
  };

  const startChatWithFriend = async (friend: FriendDto) => {
    if (!accessToken) return;
    setRespondingId(friend.id);
    try {
      const conversation = await apiRequest<ConversationDto>('/api/chat/conversations', {
        method: 'POST',
        body: JSON.stringify({ type: 'Direct', participantIds: [friend.id] }),
      }, accessToken);
      onConversationsRefresh?.();
      onSelectConversation(conversation.id);
    } finally {
      setRespondingId(null);
    }
  };

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const renderConversationList = (items: ConversationDto[], title: string, icon: React.ReactNode) => (
    <Box sx={{ mb: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, px: 2, py: 1 }}>
        {icon}
        <Typography variant="subtitle2">{title}</Typography>
      </Box>
      <List dense>
        {items.length === 0 && (
          <Typography variant="body2" color="text.secondary" sx={{ px: 2 }}>No chats yet</Typography>
        )}
        {items.map((conversation) => (
          <ListItemButton
            key={conversation.id}
            selected={conversation.id === selectedConversationId}
            onClick={() => onSelectConversation(conversation.id)}
          >
            <ListItemText
              primary={conversation.name ?? conversation.participants.find((p) => p.userId !== user?.id)?.username ?? 'Chat'}
              secondary={conversation.lastMessage ?? 'No messages yet'}
            />
          </ListItemButton>
        ))}
      </List>
    </Box>
  );

  return (
    <Box sx={{ width: 320, borderRight: 1, borderColor: 'divider', height: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Avatar src={user?.profilePictureUrl ?? undefined}>{user?.username?.[0]?.toUpperCase()}</Avatar>
          <Typography sx={{ fontWeight: 600 }}>{user?.username}</Typography>
        </Box>
        <IconButton onClick={(e) => handleNotificationsOpen(e.currentTarget)}>
          <Badge badgeContent={unreadCount} color="error">
            <NotificationsIcon />
          </Badge>
        </IconButton>
        <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={() => setAnchorEl(null)}>
          {notifications.length === 0 && <MenuItem>No notifications</MenuItem>}
          {notifications.map((notification) => {
            const isPendingInvite =
              notification.type === 'FriendInvite'
              && Boolean(notification.relatedEntityId)
              && actionableInviteIds.has(notification.id);

            return (
              <Box key={notification.id} sx={{ px: 2, py: 1, maxWidth: 320, opacity: notification.isRead ? 0.6 : 1 }}>
                <ListItemText primary={notification.title} secondary={notification.message} />
                {isPendingInvite && (
                  <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      variant="contained"
                      disabled={respondingId === notification.id}
                      onClick={() => respondToInvite(notification, true)}
                    >
                      Accept
                    </Button>
                    <Button
                      size="small"
                      variant="outlined"
                      color="inherit"
                      disabled={respondingId === notification.id}
                      onClick={() => respondToInvite(notification, false)}
                    >
                      Decline
                    </Button>
                  </Stack>
                )}
              </Box>
            );
          })}
        </Menu>
      </Box>
      <Divider />
      <Box sx={{ p: 2, display: 'flex', gap: 1 }}>
        <TextField
          size="small"
          placeholder="Search users to invite"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          fullWidth
        />
        <IconButton onClick={handleSearch} color="primary"><SearchIcon /></IconButton>
      </Box>
      {searchResults.length > 0 && (
        <List dense sx={{ maxHeight: 180, overflow: 'auto' }}>
          {searchResults.map((result) => (
            <ListItemButton key={result.id} onClick={() => sendInvite(result.id)}>
              <ListItemText
                primary={result.username}
                secondary={result.friendshipStatus === 'None' ? 'Send invite' : result.friendshipStatus}
              />
            </ListItemButton>
          ))}
        </List>
      )}
      <Divider />
      <Box sx={{ flex: 1, overflow: 'auto' }}>
        <Box sx={{ mb: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, px: 2, py: 1 }}>
            <PeopleIcon fontSize="small" />
            <Typography variant="subtitle2">Amigos</Typography>
          </Box>
          <List dense>
            {friends.length === 0 && (
              <Typography variant="body2" color="text.secondary" sx={{ px: 2 }}>
                Aún no tienes amigos
              </Typography>
            )}
            {friends.map((friend) => (
              <ListItemButton
                key={friend.id}
                sx={{ justifyContent: 'space-between' }}
                disableRipple
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, minWidth: 0 }}>
                  <Avatar src={friend.profilePictureUrl ?? undefined} sx={{ width: 32, height: 32 }}>
                    {friend.username?.[0]?.toUpperCase()}
                  </Avatar>
                  <ListItemText
                    primary={friend.username}
                    secondary={friend.fullName}
                    slotProps={{
                      primary: { noWrap: true },
                      secondary: { noWrap: true },
                    }}
                  />
                </Box>
                <Button
                  size="small"
                  variant="outlined"
                  startIcon={<ChatBubbleOutlinedIcon />}
                  disabled={respondingId === friend.id}
                  onClick={() => startChatWithFriend(friend)}
                >
                  Chatear
                </Button>
              </ListItemButton>
            ))}
          </List>
        </Box>
        <Divider sx={{ mb: 1 }} />
        {selfChat && renderConversationList([selfChat], 'Saved Messages', <PersonIcon fontSize="small" />)}
        {renderConversationList(favorites, 'Favorites', <StarIcon fontSize="small" />)}
        {renderConversationList(groups, 'Groups', <GroupIcon fontSize="small" />)}
        {renderConversationList(personalChats, 'Chats', <PersonIcon fontSize="small" />)}
      </Box>
    </Box>
  );
}
