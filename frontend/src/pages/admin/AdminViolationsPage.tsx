import { useState } from 'react';
import { motion } from 'framer-motion';
import { AlertTriangle, CheckCircle, ArrowUpCircle } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';

type Tab = 'notifications' | 'escalations';

export function AdminViolationsPage() {
  const [tab, setTab] = useState<Tab>('escalations');

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
          {/* TODO: BE chưa có API thống kê vi phạm — số liệu hiển thị '—' */}
          <div className="grid grid-cols-3 gap-4">
            {[
              { label: 'Vi phạm được xác nhận', value: '—', color: 'text-red-400', bg: 'from-red-500/15 to-red-900/20', icon: AlertTriangle },
              { label: 'Vi phạm bị bác bỏ', value: '—', color: 'text-emerald-400', bg: 'from-emerald-500/15 to-emerald-900/20', icon: CheckCircle },
              { label: 'Kháng cáo chờ xử lý', value: '—', color: 'text-orange-400', bg: 'from-orange-500/15 to-orange-900/20', icon: ArrowUpCircle },
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

          {/* Tabs */}
          {/* TODO: BE chưa có API danh sách vi phạm — số đếm tab hiển thị 0 */}
          <div className="flex items-center gap-1 border-b border-glass-border">
            {([
              ['escalations', 'Kháng cáo chờ xử lý (0)', 'text-orange-400 border-orange-400'],
              ['notifications', 'Thông báo chính thức (0)', 'text-muted border-muted'],
            ] as [Tab, string, string][]).map(([t, label, ac]) => (
              <button key={t} onClick={() => setTab(t)}
                className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${tab === t ? ac : 'text-muted border-transparent hover:text-white'}`}>
                {label}
              </button>
            ))}
          </div>

          {/* Escalations */}
          {tab === 'escalations' && (
            /* TODO: BE chưa có API danh sách kháng cáo */
            <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
              <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
              <div className="text-4xl opacity-40 mb-3">⚠️</div>
              <div className="text-muted text-sm">Chưa có dữ liệu</div>
            </div>
          )}

          {/* Official notifications (read-only) */}
          {tab === 'notifications' && (
            /* TODO: BE chưa có API danh sách thông báo vi phạm chính thức */
            <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
              <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
              <div className="text-4xl opacity-40 mb-3">⚠️</div>
              <div className="text-muted text-sm">Chưa có dữ liệu</div>
            </div>
          )}

        </main>
      </div>
    </div>
  );
}
