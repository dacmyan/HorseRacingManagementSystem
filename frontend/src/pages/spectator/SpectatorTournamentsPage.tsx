import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Search } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { getTournaments } from '../../api/publicService';
import { formatDateTime } from '../../utils/format';
import { useLanguage } from '../../context/LanguageContext';

const STATUS_CONFIG: Record<string, { label: string; color: string; dot: string }> = {
  active: { label: 'Đang diễn ra', color: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20', dot: 'bg-emerald-400' },
  upcoming: { label: 'Sắp diễn ra', color: 'text-blue-400 bg-blue-500/10 border-blue-500/20', dot: 'bg-blue-400' },
  completed: { label: 'Đã kết thúc', color: 'text-muted bg-white/5 border-glass-border', dot: 'bg-muted' },
};

export function SpectatorTournamentsPage() {
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [search, setSearch] = useState('');
  const [tournaments, setTournaments] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getTournaments()
      .then((d: any) => setTournaments(d?.result ?? (Array.isArray(d) ? d : [])))
      .catch((err) => {
        console.error(err);
        setTournaments([]);
      })
      .finally(() => setLoading(false));
  }, []);

  const filtered = tournaments.filter(t => (t.name ?? '').toLowerCase().includes(search.toLowerCase()));

  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="purple" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title={t("Giải đấu")}
            subtitle={t("Tất cả các giải đấu đang diễn ra")}
            imageUrl="/images/hero-spectator.jpg"
            imagePosition="center 50%"
          />

          <div className="flex items-center gap-2 bg-white/[0.04] border border-glass-border focus-within:border-gold/40 transition-colors rounded-lg px-3 py-2 w-64">
            <Search size={14} className="text-muted shrink-0" />
            <input value={search} onChange={e => setSearch(e.target.value)} placeholder={t("Tìm giải đấu...")} className="bg-transparent text-sm text-white placeholder:text-muted/60 outline-none w-full" />
          </div>

          {loading ? (
            <div className="text-center py-12 text-muted text-sm">{t("Đang tải danh sách giải đấu...")}</div>
          ) : filtered.length === 0 ? (
            <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
              <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
              <div className="text-4xl opacity-40 mb-3">🏆</div>
              <div className="text-muted text-sm">{t("Chưa có dữ liệu")}</div>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
              {filtered.map((tour, i) => {
                const s = tour.status?.toLowerCase() ?? 'upcoming';
                const config = STATUS_CONFIG[s] ?? STATUS_CONFIG.upcoming;
                return (
                  <motion.div
                    key={tour.tournamentId ?? i}
                    initial={{ opacity: 0, y: 12 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: i * 0.05 }}
                    className="glass-panel rounded-2xl p-5 border border-glass-border hover:border-gold/25 transition-all group relative overflow-hidden text-left"
                  >
                    <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
                    <div className="flex justify-between items-start mb-3">
                      <span className={`text-[10px] font-bold uppercase tracking-wider px-2.5 py-1 rounded-full border ${config.color} flex items-center gap-1.5`}>
                        <span className={`w-1.5 h-1.5 rounded-full ${config.dot}`} /> {t(config.label)}
                      </span>
                    </div>
                    <h3 className="text-lg font-serif text-white font-bold group-hover:text-champagne transition-colors mb-3 line-clamp-1">{tour.name}</h3>
                    <div className="space-y-1.5 text-xs text-muted pt-3 border-t border-glass-border/40">
                      <div className="flex justify-between">
                        <span>{t("Mở đăng ký:")}</span>
                        <span className="text-white font-medium">{formatDateTime(tour.registrationStartDate)}</span>
                      </div>
                      <div className="flex justify-between">
                        <span>{t("Đóng đăng ký:")}</span>
                        <span className="text-white font-medium">{formatDateTime(tour.registrationEndDate)}</span>
                      </div>
                      <div className="flex justify-between">
                        <span>{t("Ngày bắt đầu:")}</span>
                        <span className="text-white font-medium">{formatDateTime(tour.startDate)}</span>
                      </div>
                      <div className="flex justify-between">
                        <span>{t("Ngày kết thúc:")}</span>
                        <span className="text-white font-medium">{formatDateTime(tour.endDate)}</span>
                      </div>
                    </div>
                    <div className="mt-4 pt-3 border-t border-glass-border/40 flex justify-end">
                      <button 
                        onClick={() => navigate(`/spectator/tournaments/${tour.tournamentId}`)}
                        className="text-xs font-bold bg-white/5 hover:bg-white/10 text-white px-4 py-2 rounded-lg transition-colors border border-glass-border hover:border-gold/30"
                      >
                        {t("Xem chi tiết")}
                      </button>
                    </div>
                  </motion.div>
                );
              })}
            </div>
          )}

        </main>
      </div>
    </div>
  );
}
