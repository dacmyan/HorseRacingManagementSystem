import { useState } from 'react';
import { motion } from 'framer-motion';
import { Target, CheckCircle, TrendingUp, DollarSign, Search } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';

type TabType = 'all' | 'correct' | 'incorrect' | 'pending';

// TODO: BE chưa có API thống kê dự đoán — số liệu hiển thị '—'
const STATS = [
  { label: 'Tổng dự đoán', value: '—', icon: Target, color: 'text-blue-400', bg: 'from-blue-500/15 to-blue-900/20' },
  { label: 'Dự đoán đúng', value: '—', icon: CheckCircle, color: 'text-emerald-400', bg: 'from-emerald-500/15 to-emerald-900/20' },
  { label: 'Đã trả thưởng', value: '—', icon: DollarSign, color: 'text-gold', bg: 'from-gold/15 to-amber-900/20' },
  { label: 'Tỉ lệ chính xác', value: '—', icon: TrendingUp, color: 'text-purple-400', bg: 'from-purple-500/15 to-purple-900/20' },
];

const TAB_CONFIG = {
  all: { label: 'Tất cả', count: 0 },
  pending: { label: 'Chưa có kết quả', count: 0 },
  correct: { label: 'Đúng', count: 0 },
  incorrect: { label: 'Sai', count: 0 },
};

export function AdminPredictionsPage() {
  const [tab, setTab] = useState<TabType>('all');
  const [search, setSearch] = useState('');

  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="gold" />
        <Topbar />
        <main className="max-w-[1600px] mx-auto px-8 py-6 space-y-6 relative z-10">

          <PageHero
            title="Quản lý dự đoán"
            subtitle="Theo dõi và xét duyệt dự đoán khán giả"
            imageUrl="/images/hero-admin.jpg"
            imagePosition="center center"
          />

          {/* Stats */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {STATS.map((s, i) => (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.08 }}
                className="glass-panel rounded-xl p-5 relative overflow-hidden"
              >
                <div className={`absolute -top-4 -right-4 w-20 h-20 rounded-full bg-gradient-to-br ${s.bg} blur-[30px] opacity-60`} />
                <div className="relative z-10 flex items-center gap-3 mb-2">
                  <div className={`w-9 h-9 rounded-xl bg-gradient-to-br ${s.bg} border border-white/[0.08] flex items-center justify-center ${s.color}`}>
                    <s.icon size={16} />
                  </div>
                  <span className="text-[11px] uppercase tracking-wider text-muted font-bold">{s.label}</span>
                </div>
                <div className="relative z-10 text-2xl font-serif font-bold text-white">{s.value}</div>
              </motion.div>
            ))}
          </div>

          {/* Tabs + Table */}
          <div className="flex items-center gap-2 border-b border-glass-border pb-0">
            {(Object.keys(TAB_CONFIG) as TabType[]).map(t => {
              const cfg = TAB_CONFIG[t];
              return (
                <button
                  key={t}
                  onClick={() => setTab(t)}
                  className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${tab === t ? 'text-gold border-gold' : 'text-muted border-transparent hover:text-white'}`}
                >
                  {cfg.label}
                  <span className={`ml-2 px-1.5 py-0.5 rounded-full text-[11px] font-bold ${tab === t ? 'bg-gold/10 text-gold' : 'bg-white/5 text-muted'}`}>{cfg.count}</span>
                </button>
              );
            })}
            <div className="ml-auto mb-1 flex items-center gap-2 bg-white/[0.04] border border-glass-border rounded-lg px-3 py-1.5 w-56">
              <Search size={13} className="text-muted shrink-0" />
              <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm khán giả, ngựa..." className="bg-transparent text-sm text-white placeholder:text-muted/60 outline-none w-full" />
            </div>
          </div>

          {/* TODO: BE chưa có API danh sách dự đoán */}
          <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
            <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="text-4xl opacity-40 mb-3">🎯</div>
            <div className="text-muted text-sm">Chưa có dữ liệu</div>
          </motion.div>

        </main>
      </div>
    </div>
  );
}
