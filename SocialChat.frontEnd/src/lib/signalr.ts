import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { API_URL } from '@/lib/api';
import type { MessageDto, NotificationDto } from '@/lib/api';

export function createChatConnection(accessToken: string) {
  return new HubConnectionBuilder()
    .withUrl(`${API_URL}/hubs/chat`, {
      accessTokenFactory: () => accessToken,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();
}

export function createNotificationConnection(accessToken: string) {
  return new HubConnectionBuilder()
    .withUrl(`${API_URL}/hubs/notifications`, {
      accessTokenFactory: () => accessToken,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();
}

export type { MessageDto, NotificationDto };
