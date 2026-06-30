import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';
import {
  getNotifications,
  markNotificationRead,
  markAllNotificationsRead,
  deleteNotification
} from '../api/publicService';
import { getCurrentUser } from '../api/authService';

interface NotificationItem {
  id: number;
  userId: number;
  title: string;
  content: string;
  message: string;
  type: string;
  referenceId?: number;
  thumbnail?: string;
  actionUrl?: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
  isDeleted: boolean;
}

interface NotificationContextType {
  notifications: NotificationItem[];
  unreadCount: number;
  loading: boolean;
  markAsRead: (id: number) => Promise<void>;
  markAllAsRead: () => Promise<void>;
  deleteNoti: (id: number) => Promise<void>;
  fetchRecent: () => Promise<void>;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const useNotifications = () => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotifications must be used within a NotificationProvider');
  }
  return context;
};

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [unreadCount, setUnreadCount] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [toast, setToast] = useState<{ title: string; content: string } | null>(null);

  const user = getCurrentUser();

  const fetchRecent = useCallback(async () => {
    if (!user) {
      setNotifications([]);
      setUnreadCount(0);
      return;
    }
    setLoading(true);
    try {
      // Get first page (latest 10)
      const res = await getNotifications({ page: 1, pageSize: 10 });
      const items = res?.result?.items || res?.result || [];
      setNotifications(Array.isArray(items) ? items : []);

      // Also get all unread to get the accurate count
      const allRes = await getNotifications({ page: 1, pageSize: 100, isRead: false });
      const unreadItems = allRes?.result?.items || allRes?.result || [];
      const count = allRes?.result?.totalCount !== undefined 
        ? allRes.result.totalCount 
        : (Array.isArray(unreadItems) ? unreadItems.length : 0);
      
      setUnreadCount(count);
    } catch (err) {
      console.error('Failed to fetch notifications:', err);
    } finally {
      setLoading(false);
    }
  }, [user]);

  const markAsRead = async (id: number) => {
    try {
      await markNotificationRead(id);
      setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch (err) {
      console.error('Failed to mark notification as read:', err);
    }
  };

  const markAllAsRead = async () => {
    try {
      await markAllNotificationsRead();
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);
    } catch (err) {
      console.error('Failed to mark all as read:', err);
    }
  };

  const deleteNoti = async (id: number) => {
    try {
      await deleteNotification(id);
      const isUnread = notifications.find(n => n.id === id && !n.isRead);
      setNotifications(prev => prev.filter(n => n.id !== id));
      if (isUnread) {
        setUnreadCount(prev => Math.max(0, prev - 1));
      }
    } catch (err) {
      console.error('Failed to delete notification:', err);
    }
  };

  // SignalR setup
  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!user || !token) {
      if (connection) {
        connection.stop();
        setConnection(null);
      }
      return;
    }

    // Build SignalR connection
    const newConnection = new HubConnectionBuilder()
      .withUrl(`${window.location.origin}/hubs/notification`, {
        accessTokenFactory: () => token
      })
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build();

    newConnection.on('ReceiveNotification', (noti: NotificationItem) => {
      // Show dynamic toast alert
      setToast({ title: noti.title || 'Thông báo mới', content: noti.content || noti.message });
      
      // Clear toast after 5s
      setTimeout(() => {
        setToast(null);
      }, 5000);

      // Refresh list
      fetchRecent();
    });

    newConnection.start()
      .then(() => {
        console.log('[SignalR] Connected successfully.');
        setConnection(newConnection);
      })
      .catch(err => console.error('[SignalR] Connection failed:', err));

    return () => {
      newConnection.stop();
    };
  }, [user]);

  // Initial load
  useEffect(() => {
    fetchRecent();
  }, [fetchRecent]);

  return (
    <NotificationContext.Provider value={{
      notifications,
      unreadCount,
      loading,
      markAsRead,
      markAllAsRead,
      deleteNoti,
      fetchRecent
    }}>
      {children}

      {/* Floating Animated Toast */}
      {toast && (
        <div className="fixed bottom-6 right-6 z-50 max-w-sm glass-panel-elevated rounded-xl p-4 border border-gold-border/40 shadow-2xl flex flex-col gap-1.5 animate-in fade-in slide-in-from-bottom-5 duration-300 bg-[#0d1527]/95">
          <div className="flex items-center justify-between border-b border-glass-border pb-1.5">
            <div className="flex items-center gap-2">
              <span className="w-2 h-2 rounded-full bg-gold animate-pulse" />
              <span className="text-xs font-bold text-champagne uppercase tracking-wider">{toast.title}</span>
            </div>
            <button 
              onClick={() => setToast(null)}
              className="text-muted hover:text-white text-[10px] uppercase font-semibold cursor-pointer"
            >
              Đóng
            </button>
          </div>
          <p className="text-xs text-white/90 leading-relaxed font-sans">{toast.content}</p>
        </div>
      )}
    </NotificationContext.Provider>
  );
};
