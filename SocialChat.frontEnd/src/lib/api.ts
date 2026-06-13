export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export interface UserDto {
  id: string;
  username: string;
  email: string;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  fullName: string;
  profilePictureUrl?: string | null;
  isEmailVerified: boolean;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  user: UserDto;
}

export interface ConversationDto {
  id: string;
  type: string;
  name?: string | null;
  lastMessage?: string | null;
  lastMessageAt?: string | null;
  isFavorite: boolean;
  participants: {
    userId: string;
    username: string;
    fullName: string;
    profilePictureUrl?: string | null;
  }[];
}

export interface MessageDto {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
}

export interface NotificationDto {
  id: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  relatedEntityId?: string | null;
  createdAt: string;
}

export interface UserSearchResultDto {
  id: string;
  username: string;
  fullName: string;
  profilePictureUrl?: string | null;
  friendshipStatus: string;
}

export interface FriendshipDto {
  id: string;
  requesterId: string;
  requesterUsername: string;
  addresseeId: string;
  addresseeUsername: string;
  status: string;
  createdAt: string;
}

export interface FriendDto {
  id: string;
  username: string;
  fullName: string;
  profilePictureUrl?: string | null;
}

export class ApiError extends Error {
  constructor(message: string, public status: number, public errors?: Record<string, string[]>) {
    super(message);
  }
}

async function parseResponse<T>(response: Response): Promise<T> {
  const data = await response.json().catch(() => ({}));
  if (!response.ok) {
    throw new ApiError(data.message ?? 'Request failed', response.status, data.errors);
  }
  return data as T;
}

export async function apiRequest<T>(
  path: string,
  options: RequestInit = {},
  accessToken?: string | null,
): Promise<T> {
  const headers = new Headers(options.headers);
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }
  if (!(options.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
    credentials: 'include',
  });

  return parseResponse<T>(response);
}
