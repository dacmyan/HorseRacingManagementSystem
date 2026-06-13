import { Clock } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';

export function JockeyViolationsPage() {
  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="blue" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title="Vi phạm của tôi"
            subtitle="Các đơn vi phạm và khiếu nại"
            imageUrl="/images/hero-jockey.jpg"
            imagePosition="center 25%"
          />

          {/* Info */}
          <div className="glass-panel rounded-xl p-4 border border-blue-500/15 bg-blue-500/[0.02] flex items-start gap-3 relative overflow-hidden">
            <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="absolute -top-10 -right-10 w-40 h-40 rounded-full bg-gradient-to-br from-blue-500/10 to-transparent blur-[40px] pointer-events-none" />
            <div className="w-8 h-8 rounded-lg bg-blue-500/10 border border-blue-500/20 ring-1 ring-gold/20 flex items-center justify-center shrink-0 relative z-10"><Clock size={15} className="text-blue-400" /></div>
            <div className="text-xs text-muted leading-relaxed space-y-1 relative z-10">
              <div>Khi trọng tài ghi nhận vi phạm, bạn có <span className="text-white font-bold">30 phút</span> để gửi khiếu nại — trước khi kết quả cuộc đua được công bố chính thức.</div>
              <div>Trọng tài sẽ xem lại footage và ra phán quyết cuối cùng. Với án <span className="text-red-400 font-bold">truất quyền</span>, bạn có thêm <span className="text-white font-bold">48 giờ</span> để kháng cáo lên Ban tổ chức.</div>
            </div>
          </div>

          {/* TODO: BE chưa có API vi phạm jockey */}
          <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
            <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="text-4xl opacity-40 mb-3">⚠️</div>
            <div className="text-muted text-sm">Chưa có dữ liệu</div>
          </div>

        </main>
      </div>
    </div>
  );
}
