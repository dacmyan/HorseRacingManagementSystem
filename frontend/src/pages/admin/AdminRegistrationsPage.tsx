import { useState } from 'react';
import { motion } from 'framer-motion';
import { Search } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';

type TabType = 'pending' | 'approved' | 'rejected';

const TAB_CONFIG = {
  pending: { label: 'Chờ duyệt', color: 'text-yellow-400', bg: 'border-yellow-400/40 bg-yellow-400/5' },
  approved: { label: 'Đã duyệt', color: 'text-emerald-400', bg: 'border-emerald-400/40 bg-emerald-400/5' },
  rejected: { label: 'Từ chối', color: 'text-red-400', bg: 'border-red-400/40 bg-red-400/5' },
};

export function AdminRegistrationsPage() {
  const [tab, setTab] = useState<TabType>('pending');
  const [search, setSearch] = useState('');

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
            {/* TODO: BE chưa có API danh sách đăng ký — số đếm tab hiển thị 0 */}
            {(['pending', 'approved', 'rejected'] as TabType[]).map(t => {
              const cfg = TAB_CONFIG[t];
              return (
                <button
                  key={t}
                  onClick={() => setTab(t)}
                  className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${
                    tab === t ? `${cfg.color} border-current` : 'text-muted border-transparent hover:text-white'
                  }`}
                >
                  {cfg.label}
                  <span className={`ml-2 px-2 py-0.5 rounded-full text-[11px] font-bold ${tab === t ? cfg.bg + ' ' + cfg.color : 'bg-white/5 text-muted'}`}>
                    0
                  </span>
                </button>
              );
            })}
            <div className="ml-auto mb-1 flex items-center gap-2 bg-white/[0.04] border border-glass-border rounded-lg px-3 py-1.5 w-56">
              <Search size={13} className="text-muted shrink-0" />
              <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm ngựa, chủ ngựa..." className="bg-transparent text-sm text-white placeholder:text-muted/60 outline-none w-full" />
            </div>
          </div>

          {/* Table */}
          {/* TODO: BE chưa có API danh sách đăng ký thi đấu */}
          <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
            <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="text-4xl opacity-40 mb-3">📝</div>
            <div className="text-muted text-sm">Chưa có dữ liệu</div>
          </motion.div>

        </main>
      </div>
    </div>
  );
}
