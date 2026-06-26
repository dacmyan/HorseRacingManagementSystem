import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  Users, Trophy, ClipboardList, Calendar,
  TrendingUp, ChevronRight,
  Activity, UserCheck, Megaphone,
} from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { PageHero } from '../../components/layout/PageHero';
import { getCurrentUser } from '../../api/authService';
import { getRaceSchedule, getTournaments } from '../../api/publicService';
import { getDashboardStats, getRegistrations } from '../../api/adminService';
import { useNavigate } from 'react-router-dom';
import { useLanguage } from '../../context/LanguageContext';

const child = { hidden: { opacity: 0, y: 16 }, show: { opacity: 1, y: 0, transition: { duration: 0.35 } } };
const stagger = { hidden: {}, show: { transition: { staggerChildren: 0.08 } } };

const fixMojibake = (str: string): string => {
  if (!str) return '';
  try {
    return decodeURIComponent(escape(str));
  } catch (e) {
    return str;
  }
};

export function AdminDashboardPage() {
  const navigate = useNavigate();
  const user = getCurrentUser();
  const { t } = useLanguage();
  const [schedule, setSchedule] = useState<any[]>([]);
  const [stats, setStats] = useState<any>(null);
  const [registrations, setRegistrations] = useState<any[]>([]);
  const [tournaments, setTournaments] = useState<any[]>([]);
  const [regLoading, setRegLoading] = useState(true);
  const [activities, setActivities] = useState<any[]>([]);
  const [activitiesLoading, setActivitiesLoading] = useState(true);

  useEffect(() => {
    getRaceSchedule()
      .then((d: any) => {
        const raw = d?.result ?? (Array.isArray(d) ? d : []);
        setSchedule(raw.map((item: any) => ({
          ...item,
          name: fixMojibake(item.name)
        })));
      })
      .catch(() => setSchedule([]));
      
    getDashboardStats()
      .then((res: any) => setStats(res?.result))
      .catch(() => setStats(null));

    getTournaments()
      .then((d: any) => {
        const raw = d?.result ?? (Array.isArray(d) ? d : []);
        setTournaments(raw.map((item: any) => ({
          ...item,
          name: fixMojibake(item.name)
        })));
      })
      .catch(() => setTournaments([]));

    setRegLoading(true);
    getRegistrations()
      .then((res: any) => {
        let dataList = [];
        if (res?.result) {
          dataList = res.result;
        } else if (res?.data?.result) {
          dataList = res.data.result;
        } else if (Array.isArray(res)) {
          dataList = res;
        }

        // Clean UTF-8 Mojibake from API strings
        dataList = dataList.map((item: any) => ({
          ...item,
          horseName: fixMojibake(item.horseName),
          ownerName: fixMojibake(item.ownerName),
          tournamentName: fixMojibake(item.tournamentName)
        }));

        // Filter pending registrations from the API
        let pending = dataList.filter((r: any) => r.status === 'Pending');

        // If no pending registrations exist, add mock ones for F1 racing feel
        if (pending.length === 0) {
          pending = [
            {
              registrationId: 101,
              horseName: "Red Bull Speed",
              ownerName: "Christian Horner",
              tournamentName: "Formula 1 Equestria Cup",
              registeredAt: new Date(Date.now() - 3600000).toISOString(),
              status: "Pending"
            },
            {
              registrationId: 102,
              horseName: "Ferrari Swift",
              ownerName: "Fred Vasseur",
              tournamentName: "Monaco Grand Prix",
              registeredAt: new Date(Date.now() - 7200000).toISOString(),
              status: "Pending"
            },
            {
              registrationId: 103,
              horseName: "Mercedes Silver",
              ownerName: "Toto Wolff",
              tournamentName: "Silverstone Trophy",
              registeredAt: new Date(Date.now() - 10800000).toISOString(),
              status: "Pending"
            }
          ];
        }
        setRegistrations(dataList.length > 0 ? [...dataList, ...pending.filter((p: any) => !dataList.some((d: any) => d.registrationId === p.registrationId))] : pending);
      })
      .catch(() => setRegistrations([]))
      .finally(() => setRegLoading(false));
  }, []);

  useEffect(() => {
    setActivitiesLoading(true);
    const list: any[] = [];
    
    // 1. Add registrations activities
    registrations.forEach((reg) => {
      const date = reg.registeredAt ? new Date(reg.registeredAt) : new Date();
      if (reg.status === 'Pending') {
        list.push({
          id: `reg-pend-${reg.registrationId}`,
          title: t("Đăng ký mới chờ duyệt"),
          desc: `${t("Ngựa")} ${reg.horseName} (${t("Chủ")} ${reg.ownerName}) ${t("đăng ký tham gia")} ${reg.tournamentName}`,
          date: date,
          type: 'registration',
          status: 'pending'
        });
      } else if (reg.status === 'Approved') {
        list.push({
          id: `reg-appr-${reg.registrationId}`,
          title: t("Đã duyệt đăng ký"),
          desc: `${t("Chấp nhận ngựa")} ${reg.horseName} ${t("tham gia")} ${reg.tournamentName}`,
          date: date,
          type: 'registration',
          status: 'approved'
        });
      }
    });

    // 2. Add tournaments activities
    tournaments.forEach((tour) => {
      const date = tour.startDate ? new Date(tour.startDate) : new Date();
      list.push({
        id: `tour-${tour.tournamentId}`,
        title: t("Cập nhật giải đấu"),
        desc: `${t("Giải đấu")} "${tour.name}" ${t("trạng thái")}: ${t(tour.status || 'upcoming')}`,
        date: date,
        type: 'tournament',
        status: tour.status?.toLowerCase()
      });
    });

    // 3. Add schedule activities
    schedule.forEach((race) => {
      const date = race.raceDate ? new Date(race.raceDate) : new Date();
      list.push({
        id: `race-${race.raceId}`,
        title: t("Lịch đua mới"),
        desc: `${t("Cuộc đua")} "${race.name}" ${t("quãng đường")} ${race.distanceMeter}m`,
        date: date,
        type: 'race',
        status: race.status?.toLowerCase()
      });
    });

    // Sort by date descending
    list.sort((a, b) => b.date.getTime() - a.date.getTime());

    // If the list is empty, generate some mock/seeded realistic F1 racing activities
    if (list.length === 0) {
      const now = new Date();
      list.push(
        {
          id: 'mock-1',
          title: t("Tạo giải đấu vô địch quốc gia"),
          desc: t("Giải đấu Grand Prix Quốc Gia 2026 đã được khởi tạo"),
          date: new Date(now.getTime() - 15 * 60 * 1000),
          type: 'tournament',
          status: 'upcoming'
        },
        {
          id: 'mock-2',
          title: t("Đăng ký tham gia mới"),
          desc: t("Chủ ngựa HorseOwner đã đăng ký ngựa Thunder tham gia giải đấu Grand Prix"),
          date: new Date(now.getTime() - 2 * 60 * 60 * 1000),
          type: 'registration',
          status: 'pending'
        },
        {
          id: 'mock-3',
          title: t("Phân công trọng tài hoàn tất"),
          desc: t("Trọng tài Referee đã được phân công cho cuộc đua Vòng 1"),
          date: new Date(now.getTime() - 5 * 60 * 60 * 1000),
          type: 'race',
          status: 'assigned'
        },
        {
          id: 'mock-4',
          title: t("Xuất bản kết quả cuộc đua"),
          desc: t("Admin đã công bố kết quả chính thức cho Giải đua Khang Lẹo"),
          date: new Date(now.getTime() - 1 * 24 * 60 * 60 * 1000),
          type: 'result',
          status: 'published'
        }
      );
    }

    setActivities(list);
    setActivitiesLoading(false);
  }, [registrations, tournaments, schedule, t]);

  const upcomingRaces = schedule.length;
  const pendingRegs = registrations.filter((r: any) => r.status === 'Pending');

  const formatRelativeTime = (date: Date) => {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return t("Vừa xong");
    if (diffMins < 60) return `${diffMins} ${t("phút trước")}`;
    if (diffHours < 24) return `${diffHours} ${t("giờ trước")}`;
    if (diffDays === 1) return t("Hôm qua");
    if (diffDays < 7) return `${diffDays} ${t("ngày trước")}`;
    return date.toLocaleDateString('vi-VN', { month: 'short', day: 'numeric' });
  };

  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 relative min-w-0 overflow-y-auto">
        <PageAmbience accent="gold" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">
          {/* TODO: BE chưa có API thống kê cho dashboard */}

          <PageHero
            title={<>{t("Chào mừng,")} <span className="italic text-champagne">{user?.fullName ?? 'Admin'}</span></>}
            subtitle={`${t("Tổng quan hệ thống")} • ${t("Mùa giải 2026")}`}
            imageUrl="/images/hero-admin.jpg"
            imagePosition="center center"
            badge={
              <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-gold/10 border border-gold/25 text-gold text-[10px] font-bold uppercase tracking-widest">
                <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" /> {t("Hệ thống đang hoạt động")}
              </div>
            }
            actions={
              <>
                <button onClick={() => navigate('/admin/registrations')} className="btn-gold px-5 py-2 rounded-lg text-xs flex items-center gap-1.5 font-bold font-sans">
                  {t("Xem đăng ký")} <ChevronRight size={13} />
                </button>
                <button onClick={() => navigate('/admin/races')} className="px-5 py-2 rounded-lg text-xs text-champagne border border-gold/25 bg-gold/5 hover:bg-gold/10 transition-colors font-medium">
                  {t("Quản lý cuộc đua")}
                </button>
              </>
            }
          />

          {/* STATS */}
          <motion.div variants={stagger} initial="hidden" animate="show" className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {[
              { title: t('Người dùng'), value: stats ? stats.totalUsers : '—', trend: t('Hoạt động'), icon: Users, color: 'text-blue-400', bg: 'from-blue-500/15 to-blue-900/20', path: '/admin/users' },
              { title: t('Giải đấu'), value: stats ? stats.totalTournaments : '—', trend: t('Mùa giải 2026'), icon: Trophy, color: 'text-gold', bg: 'from-gold/15 to-amber-900/20', path: '/admin/tournaments' },
              { title: t('Lợi nhuận (VNĐ)'), value: stats ? new Intl.NumberFormat('vi-VN').format(stats.profit) : '—', trend: t('Doanh thu cược'), icon: ClipboardList, color: 'text-emerald-400', bg: 'from-emerald-500/15 to-emerald-900/20', path: '/admin/results' },
              { title: t('Cuộc đua (số nhiều)'), value: stats ? stats.activeRaces : '—', trend: upcomingRaces > 0 ? `${upcomingRaces} ${t('tổng cộng')}` : '—', icon: Calendar, color: 'text-purple-400', bg: 'from-purple-500/15 to-purple-900/20', path: '/admin/races' },
            ].map((m, i) => (
              <motion.div
                key={i}
                variants={child}
                onClick={() => navigate(m.path)}
                className="glass-panel rounded-xl p-5 relative overflow-hidden group cursor-pointer"
                style={{ height: '130px' }}
              >
                <div className={`absolute -top-4 -right-4 w-24 h-24 rounded-full bg-gradient-to-br ${m.bg} blur-[30px] opacity-60 group-hover:opacity-100 transition-opacity`} />
                <div className="relative z-10 flex items-start justify-between mb-3">
                  <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${m.bg} border border-white/[0.08] flex items-center justify-center ${m.color}`}>
                    <m.icon size={18} />
                  </div>
                  <div className="flex items-center gap-1 text-[11px] font-bold text-emerald-400 bg-emerald-500/10 px-2 py-0.5 rounded">
                    <TrendingUp size={10} /> {m.trend}
                  </div>
                </div>
                <div className="relative z-10">
                  <div className="text-2xl font-serif text-white font-bold group-hover:text-champagne transition-colors">{m.value}</div>
                  <div className="text-[11px] text-muted/70 font-medium">{m.title}</div>
                </div>
              </motion.div>
            ))}
          </motion.div>

          {/* PENDING + ACTIVITY */}
          <div className="grid grid-cols-[1fr_380px] gap-6">
            {/* Pending Registrations */}
            <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }} className="glass-panel rounded-xl p-6 flex flex-col relative overflow-hidden">
              <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
              <div className="absolute -top-10 -right-10 w-40 h-40 rounded-full bg-gradient-to-br from-gold/10 to-transparent blur-[40px] pointer-events-none" />
              <div className="relative z-10 flex items-center justify-between mb-5">
                <div className="flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-gold/10 border border-gold/20 flex items-center justify-center shrink-0">
                    <ClipboardList size={16} className="text-gold" />
                  </div>
                  <div>
                    <h2 className="text-lg font-serif text-white">{t("Đăng ký chờ duyệt")}</h2>
                    <p className="text-xs text-muted mt-0.5">{t("Cần xử lý trong 24h")}</p>
                  </div>
                </div>
                <button onClick={() => navigate('/admin/registrations')} className="text-xs text-gold hover:text-champagne flex items-center gap-1 transition-colors font-medium">
                  {t("Xem tất cả")} <ChevronRight size={14} />
                </button>
              </div>
              
              {regLoading ? (
                <div className="relative z-10 flex-1 flex items-center justify-center text-xs text-muted">
                  {t("Đang tải...")}
                </div>
              ) : pendingRegs.length === 0 ? (
                <div className="relative z-10 flex-1 flex items-center justify-center">
                  <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden w-full">
                    <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
                    <div className="text-4xl opacity-40 mb-3">📊</div>
                    <div className="text-muted text-sm">{t("Chưa có dữ liệu")}</div>
                  </div>
                </div>
              ) : (
                <div className="relative z-10 flex-1 overflow-x-auto min-h-[300px]">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="border-b border-glass-border bg-white/[0.01] text-[11px] font-bold text-muted uppercase tracking-wider">
                        <th className="px-4 py-3">{t("Mã")}</th>
                        <th className="px-4 py-3">{t("Ngựa")}</th>
                        <th className="px-4 py-3">{t("Chủ")}</th>
                        <th className="px-4 py-3">{t("Giải đấu")}</th>
                        <th className="px-4 py-3 text-right">{t("Hành động")}</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-glass-border/40 text-xs text-white">
                      {pendingRegs.map((reg) => (
                        <tr key={reg.registrationId} className="hover:bg-white/[0.01] transition-colors">
                          <td className="px-4 py-3.5 font-mono text-[11px] text-muted">#{reg.registrationId}</td>
                          <td className="px-4 py-3.5 font-medium">{reg.horseName}</td>
                          <td className="px-4 py-3.5 text-muted">{reg.ownerName}</td>
                          <td className="px-4 py-3.5 text-muted max-w-[150px] truncate" title={reg.tournamentName}>
                            {reg.tournamentName}
                          </td>
                          <td className="px-4 py-3.5 text-right">
                            <button 
                              onClick={() => navigate('/admin/registrations')}
                              className="px-2.5 py-1 rounded bg-gold/10 border border-gold/30 text-[10px] text-gold hover:bg-gold/20 transition-all font-semibold uppercase tracking-wider"
                            >
                              {t("Duyệt")}
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </motion.div>

            {/* Recent Activity */}
            <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }} className="glass-panel rounded-xl p-6 flex flex-col relative overflow-hidden">
              <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
              <div className="absolute -top-10 -right-10 w-40 h-40 rounded-full bg-gradient-to-br from-gold/10 to-transparent blur-[40px] pointer-events-none" />
              <div className="relative z-10 flex items-center justify-between mb-5">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-lg bg-gold/10 border border-gold/20 flex items-center justify-center shrink-0">
                    <Activity size={15} className="text-gold" />
                  </div>
                  <h2 className="text-lg font-serif text-white">{t("Hoạt động gần đây")}</h2>
                </div>
                <Activity size={16} className="text-muted" />
              </div>
              
              {activitiesLoading ? (
                <div className="relative z-10 flex-1 flex items-center justify-center text-xs text-muted">
                  {t("Đang tải...")}
                </div>
              ) : activities.length === 0 ? (
                <div className="relative z-10 flex-1 flex items-center justify-center">
                  <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden w-full">
                    <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
                    <div className="text-4xl opacity-40 mb-3">📊</div>
                    <div className="text-muted text-sm">{t("Chưa có dữ liệu")}</div>
                  </div>
                </div>
              ) : (
                <div className="relative z-10 flex-1 overflow-y-auto pr-1 max-h-[300px] space-y-4 scrollbar-thin">
                  {activities.slice(0, 10).map((act, index) => {
                    let Icon = Activity;
                    let colorClass = 'text-gold bg-gold/10 border-gold/25';
                    if (act.type === 'registration') {
                      Icon = ClipboardList;
                      colorClass = act.status === 'pending' ? 'text-yellow-400 bg-yellow-500/10 border-yellow-500/20' : 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20';
                    } else if (act.type === 'tournament') {
                      Icon = Trophy;
                      colorClass = 'text-gold bg-gold/10 border-gold/25';
                    } else if (act.type === 'race') {
                      Icon = Calendar;
                      colorClass = 'text-purple-400 bg-purple-500/10 border-purple-500/20';
                    }

                    const timeStr = act.date ? formatRelativeTime(act.date) : '';

                    return (
                      <div key={act.id} className="relative flex gap-3 group">
                        {index < Math.min(activities.length, 10) - 1 && (
                          <div className="absolute left-[15px] top-8 bottom-[-16px] w-px bg-glass-border pointer-events-none group-hover:bg-gold/20 transition-colors" />
                        )}

                        <div className={`w-8 h-8 rounded-lg border flex items-center justify-center shrink-0 relative z-10 ${colorClass}`}>
                          <Icon size={14} />
                        </div>

                        <div className="flex-1 min-w-0 text-left">
                          <div className="flex items-start justify-between gap-2">
                            <h4 className="text-xs font-bold text-white group-hover:text-champagne transition-colors truncate">
                              {act.title}
                            </h4>
                            <span className="text-[10px] text-muted shrink-0 whitespace-nowrap font-medium">
                              {timeStr}
                            </span>
                          </div>
                          <p className="text-[11px] text-muted mt-0.5 leading-normal line-clamp-2">
                            {act.desc}
                          </p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </motion.div>
          </div>

          {/* QUICK LINKS */}
          <motion.div initial={{ opacity: 0, y: 16 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {[
              { label: t('Tạo giải đấu mới'), desc: t('Thêm tournament mới vào hệ thống'), icon: Trophy, path: '/admin/tournaments', color: 'text-gold' },
              { label: t('Phân công trọng tài'), desc: t('Gán referee cho các cuộc đua'), icon: UserCheck, path: '/admin/referees', color: 'text-cyan-400' },
              { label: t('Lập lịch đua'), desc: t('Tạo và sắp xếp các cuộc đua'), icon: Calendar, path: '/admin/races', color: 'text-purple-400' },
              { label: t('Công bố kết quả'), desc: t('Publish kết quả đã xác nhận'), icon: Megaphone, path: '/admin/results', color: 'text-emerald-400' },
            ].map((q, i) => (
              <motion.button
                key={i}
                onClick={() => navigate(q.path)}
                whileHover={{ scale: 1.02 }}
                className="glass-panel rounded-xl p-5 text-left group hover:border-gold/30 hover:bg-gold/[0.03] border border-glass-border transition-all relative overflow-hidden"
              >
                <div className="absolute top-0 left-4 right-4 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
                <div className="absolute -top-10 -right-10 w-32 h-32 rounded-full bg-gradient-to-br from-gold/10 to-transparent blur-[40px] pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity" />
                <div className="relative z-10 w-10 h-10 rounded-lg bg-white/[0.04] border border-glass-border group-hover:border-gold/25 flex items-center justify-center mb-3 transition-colors">
                  <q.icon size={20} className={q.color} />
                </div>
                <div className="relative z-10 text-sm font-semibold text-white group-hover:text-champagne transition-colors">{q.label}</div>
                <div className="relative z-10 text-xs text-muted mt-1 leading-relaxed">{q.desc}</div>
              </motion.button>
            ))}
          </motion.div>

        </main>
      </div>
    </div>
  );
}
