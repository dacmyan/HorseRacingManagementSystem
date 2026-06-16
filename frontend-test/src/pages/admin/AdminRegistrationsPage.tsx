import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Search } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { getRegistrations } from '../../api/adminService';

type TabType = 'pending' | 'approved' | 'rejected';

const TAB_CONFIG = {
  pending: { label: 'Chờ duyệt', color: 'text-yellow-400', bg: 'border-yellow-400/40 bg-yellow-400/5', statusValue: 'Pending' },
  approved: { label: 'Đã duyệt', color: 'text-emerald-400', bg: 'border-emerald-400/40 bg-emerald-400/5', statusValue: 'Approved' },
  rejected: { label: 'Từ chối', color: 'text-red-400', bg: 'border-red-400/40 bg-red-400/5', statusValue: 'Rejected' },
};

interface Registration {
  registrationId: number;
  tournamentId: number;
  tournamentName: string;
  horseId: number;
  horseName: string;
  ownerName: string;
  status: string;
  registeredAt: string;
}

export function AdminRegistrationsPage() {
  const [tab, setTab] = useState<TabType>('pending');
  const [search, setSearch] = useState('');
  const [registrations, setRegistrations] = useState<Registration[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getRegistrations()
      .then(res => {
        if (res.data && res.data.result) {
          setRegistrations(res.data.result);
        } else {
          setRegistrations([]);
        }
        setLoading(false);
      })
      .catch(err => {
        console.error(err);
        setError('Không thể lấy danh sách đăng ký');
        setLoading(false);
      });
  }, []);

  // Filter registrations by tab status and search query
  const filteredRegistrations = registrations.filter(r => {
    const statusMatch = r.status?.toLowerCase() === TAB_CONFIG[tab].statusValue.toLowerCase();
    const query = search.toLowerCase();
    const searchMatch = !search || 
      r.horseName?.toLowerCase().includes(query) || 
      r.ownerName?.toLowerCase().includes(query) ||
      r.tournamentName?.toLowerCase().includes(query);
    return statusMatch && searchMatch;
  });

  const getCount = (t: TabType) => {
    const statusVal = TAB_CONFIG[t].statusValue.toLowerCase();
    return registrations.filter(r => r.status?.toLowerCase() === statusVal).length;
  };

  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 relative min-w-0 overflow-y-auto">
        <PageAmbience accent="gold" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title="Duyệt đăng ký"
            subtitle="Xét duyệt đăng ký tham gia thi đấu"
            imageUrl="/images/hero-admin.jpg"
            imagePosition="center center"
          />

          {/* Tabs */}
          <div className="flex items-center gap-2 border-b border-glass-border pb-0">
            {(['pending', 'approved', 'rejected'] as TabType[]).map(t => {
              const cfg = TAB_CONFIG[t];
              const isActive = tab === t;
              return (
                <button
                  key={t}
                  onClick={() => setTab(t)}
                  className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${
                    isActive ? `${cfg.color} border-current` : 'text-muted border-transparent hover:text-white'
                  }`}
                >
                  {cfg.label}
                  <span className={`ml-2 px-2 py-0.5 rounded-full text-[11px] font-bold ${isActive ? cfg.bg + ' ' + cfg.color : 'bg-white/5 text-muted'}`}>
                    {getCount(t)}
                  </span>
                </button>
              );
            })}
            <div className="ml-auto mb-1 flex items-center gap-2 bg-white/[0.04] border border-glass-border rounded-lg px-3 py-1.5 w-56">
              <Search size={13} className="text-muted shrink-0" />
              <input 
                value={search} 
                onChange={e => setSearch(e.target.value)} 
                placeholder="Tìm ngựa, chủ ngựa..." 
                className="bg-transparent text-sm text-white placeholder:text-muted/60 outline-none w-full" 
              />
            </div>
          </div>

          {/* Error state */}
          {error && (
            <div className="p-4 bg-red-500/10 border border-red-500/20 text-red-400 rounded-lg text-sm">
              {error}
            </div>
          )}

          {/* Loading state */}
          {loading ? (
            <div className="glass-panel rounded-xl p-12 text-center text-muted">
              Đang tải danh sách đăng ký...
            </div>
          ) : filteredRegistrations.length === 0 ? (
            <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
              <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
              <div className="text-4xl opacity-40 mb-3">📝</div>
              <div className="text-muted text-sm">Chưa có dữ liệu đăng ký nào</div>
            </motion.div>
          ) : (
            <motion.div 
              initial={{ opacity: 0, y: 16 }} 
              animate={{ opacity: 1, y: 0 }} 
              className="glass-panel rounded-xl overflow-hidden"
            >
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="border-b border-glass-border bg-white/[0.02] text-xs font-semibold text-muted uppercase tracking-wider">
                      <th className="px-6 py-4">Mã ĐK</th>
                      <th className="px-6 py-4">Ngựa</th>
                      <th className="px-6 py-4">Chủ ngựa</th>
                      <th className="px-6 py-4">Giải đấu</th>
                      <th className="px-6 py-4">Ngày đăng ký</th>
                      <th className="px-6 py-4">Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-glass-border/40 text-sm text-white">
                    {filteredRegistrations.map((reg) => (
                      <tr key={reg.registrationId} className="hover:bg-white/[0.01] transition-colors">
                        <td className="px-6 py-4 font-mono text-xs text-muted">#{reg.registrationId}</td>
                        <td className="px-6 py-4 font-medium">{reg.horseName}</td>
                        <td className="px-6 py-4 text-muted">{reg.ownerName}</td>
                        <td className="px-6 py-4 text-muted">{reg.tournamentName}</td>
                        <td className="px-6 py-4 text-muted">
                          {reg.registeredAt ? new Date(reg.registeredAt).toLocaleDateString('vi-VN') : ''}
                        </td>
                        <td className="px-6 py-4">
                          <span className={`px-2 py-1 rounded text-xs font-semibold ${
                            reg.status === 'Pending' ? 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20' :
                            reg.status === 'Approved' ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' :
                            'bg-red-500/10 text-red-400 border border-red-500/20'
                          }`}>
                            {reg.status === 'Pending' ? 'Chờ duyệt' : reg.status === 'Approved' ? 'Đã duyệt' : 'Từ chối'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </motion.div>
          )}

        </main>
      </div>
    </div>
  );
}
