import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { AlertTriangle, CheckCircle, ArrowUpCircle } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { getViolations } from '../../api/adminService';

type Tab = 'notifications' | 'escalations';

interface Violation {
  violationId: number;
  raceId: number;
  raceName: string;
  type: string;
  note: string;
  penalty: string;
  createdAt: string;
}

export function AdminViolationsPage() {
  const [tab, setTab] = useState<Tab>('notifications');
  const [violations, setViolations] = useState<Violation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getViolations()
      .then(res => {
        if (res.data && res.data.result) {
          setViolations(res.data.result);
        } else {
          setViolations([]);
        }
        setLoading(false);
      })
      .catch(err => {
        console.error(err);
        setError('Không thể tải danh sách vi phạm');
        setLoading(false);
      });
  }, []);

  const totalConfirmed = violations.length;
  // Let's assume violations containing "kháng cáo" or "khiếu nại" or "appeal" are escalations
  const escalations = violations.filter(v => 
    v.note?.toLowerCase().includes('kháng cáo') || 
    v.note?.toLowerCase().includes('khiếu nại') ||
    v.note?.toLowerCase().includes('appeal')
  );
  
  const notifications = violations.filter(v => !escalations.includes(v));

  const totalEscalations = escalations.length;
  const totalNotifications = notifications.length;

  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="gold" />
        <Topbar />
        <main className="max-w-[1600px] mx-auto px-8 py-6 space-y-6 relative z-10">

          <PageHero
            title="Xử lý vi phạm"
            subtitle="Kháng cáo và quyết định chính thức"
            imageUrl="/images/hero-admin.jpg"
            imagePosition="center center"
          />

          {/* Flow */}
          <div className="glass-panel rounded-xl p-4 border border-glass-border relative overflow-hidden">
            <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="absolute -top-10 -right-10 w-40 h-40 rounded-full bg-gradient-to-br from-gold/10 to-transparent blur-[40px] pointer-events-none" />
            <div className="relative z-10 flex items-center gap-2 text-xs flex-wrap">
              <span className="text-muted font-bold shrink-0">Quy trình:</span>
              {[
                { label: 'Trọng tài ghi nhận',       active: false },
                { label: '→', sep: true },
                { label: 'Jockey khiếu nại (30 phút)', active: false },
                { label: '→', sep: true },
                { label: 'Trọng tài ra quyết định',   active: false },
                { label: '→', sep: true },
                { label: 'Admin nhận thông báo',       active: false, note: true },
                { label: '→', sep: true },
                { label: 'Kháng cáo án nặng (48h)',   active: true },
              ].map((s, i) =>
                s.sep ? <span key={i} className="text-muted/30">→</span>
                  : <span key={i} className={`px-2.5 py-1 rounded-lg border text-white/80 ${s.active ? 'bg-gold/10 border-gold/20 text-gold font-bold' : s.note ? 'bg-blue-500/10 border-blue-500/20 text-blue-400' : 'bg-white/[0.03] border-glass-border'}`}>{s.label}</span>
              )}
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-3 gap-4">
            {[
              { label: 'Vi phạm được xác nhận', value: loading ? '...' : totalConfirmed, color: 'text-red-400', bg: 'from-red-500/15 to-red-900/20', icon: AlertTriangle },
              { label: 'Vi phạm bị bác bỏ', value: '0', color: 'text-emerald-400', bg: 'from-emerald-500/15 to-emerald-900/20', icon: CheckCircle },
              { label: 'Kháng cáo chờ xử lý', value: loading ? '...' : totalEscalations, color: 'text-orange-400', bg: 'from-orange-500/15 to-orange-900/20', icon: ArrowUpCircle },
            ].map((s, i) => (
              <motion.div key={i} initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.07 }}
                className="glass-panel rounded-xl p-5 relative overflow-hidden border border-glass-border hover:border-gold/30 transition-all">
                <div className="absolute top-0 left-4 right-4 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
                <div className={`absolute -top-4 -right-4 w-20 h-20 rounded-full bg-gradient-to-br ${s.bg} blur-[30px] opacity-60 pointer-events-none`} />
                <div className={`w-9 h-9 rounded-xl bg-gradient-to-br ${s.bg} border border-white/[0.08] flex items-center justify-center ${s.color} mb-3 relative z-10`}>
                  <s.icon size={16} />
                </div>
                <div className="relative z-10 text-2xl font-serif font-bold text-white">{s.value}</div>
                <div className="relative z-10 text-[11px] text-muted font-medium mt-0.5">{s.label}</div>
              </motion.div>
            ))}
          </div>

          {/* Error Message */}
          {error && (
            <div className="p-4 bg-red-500/10 border border-red-500/20 text-red-400 rounded-lg text-sm">
              {error}
            </div>
          )}

          {/* Tabs */}
          <div className="flex items-center gap-1 border-b border-glass-border">
            {( [
              ['notifications', `Thông báo chính thức (${loading ? 0 : totalNotifications})`, 'text-red-400 border-red-400'],
              ['escalations', `Kháng cáo chờ xử lý (${loading ? 0 : totalEscalations})`, 'text-orange-400 border-orange-400'],
            ] as [Tab, string, string][]).map(([t, label, ac]) => (
              <button key={t} onClick={() => setTab(t)}
                className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${tab === t ? ac : 'text-muted border-transparent hover:text-white'}`}>
                {label}
              </button>
            ))}
          </div>

          {/* Loading */}
          {loading ? (
            <div className="glass-panel rounded-xl p-12 text-center text-muted">
              Đang tải danh sách vi phạm...
            </div>
          ) : (
            <div>
              {tab === 'escalations' && (
                totalEscalations === 0 ? (
                  <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
                    <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
                    <div className="text-4xl opacity-40 mb-3">⚠️</div>
                    <div className="text-muted text-sm">Không có khiếu nại/kháng cáo nào chờ xử lý</div>
                  </div>
                ) : (
                  <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="glass-panel rounded-xl overflow-hidden">
                    <div className="overflow-x-auto">
                      <table className="w-full text-left border-collapse">
                        <thead>
                          <tr className="border-b border-glass-border bg-white/[0.02] text-xs font-semibold text-muted uppercase tracking-wider">
                            <th className="px-6 py-4">Mã VP</th>
                            <th className="px-6 py-4">Cuộc đua</th>
                            <th className="px-6 py-4">Loại vi phạm</th>
                            <th className="px-6 py-4">Mô tả / Kháng nghị</th>
                            <th className="px-6 py-4">Hình phạt dự kiến</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-glass-border/40 text-sm text-white">
                          {escalations.map((v) => (
                            <tr key={v.violationId} className="hover:bg-white/[0.01] transition-colors">
                              <td className="px-6 py-4 font-mono text-xs text-muted">#{v.violationId}</td>
                              <td className="px-6 py-4 font-medium">{v.raceName}</td>
                              <td className="px-6 py-4 text-orange-400 font-semibold">{v.type}</td>
                              <td className="px-6 py-4 text-muted">{v.note}</td>
                              <td className="px-6 py-4">
                                <span className="px-2 py-1 bg-red-500/10 text-red-400 border border-red-500/20 rounded text-xs font-semibold">
                                  {v.penalty}
                                </span>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </motion.div>
                )
              )}

              {tab === 'notifications' && (
                totalNotifications === 0 ? (
                  <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
                    <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
                    <div className="text-4xl opacity-40 mb-3">⚠️</div>
                    <div className="text-muted text-sm">Chưa có thông báo vi phạm chính thức</div>
                  </div>
                ) : (
                  <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="glass-panel rounded-xl overflow-hidden">
                    <div className="overflow-x-auto">
                      <table className="w-full text-left border-collapse">
                        <thead>
                          <tr className="border-b border-glass-border bg-white/[0.02] text-xs font-semibold text-muted uppercase tracking-wider">
                            <th className="px-6 py-4">Mã VP</th>
                            <th className="px-6 py-4">Cuộc đua</th>
                            <th className="px-6 py-4">Loại vi phạm</th>
                            <th className="px-6 py-4">Mô tả</th>
                            <th className="px-6 py-4">Hình phạt</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-glass-border/40 text-sm text-white">
                          {notifications.map((v) => (
                            <tr key={v.violationId} className="hover:bg-white/[0.01] transition-colors">
                              <td className="px-6 py-4 font-mono text-xs text-muted">#{v.violationId}</td>
                              <td className="px-6 py-4 font-medium">{v.raceName}</td>
                              <td className="px-6 py-4 text-red-400 font-semibold">{v.type}</td>
                              <td className="px-6 py-4 text-muted">{v.note}</td>
                              <td className="px-6 py-4">
                                <span className="px-2 py-1 bg-red-500/10 text-red-400 border border-red-500/20 rounded text-xs font-semibold">
                                  {v.penalty}
                                </span>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </motion.div>
                )
              )}
            </div>
          )}

        </main>
      </div>
    </div>
  );
}
