import { useState } from 'react';
import { motion } from 'framer-motion';
import { Plus, Flag } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { createRace } from '../../api/adminService';
import { parseApiError } from '../../api/authService';

const INPUT = 'w-full bg-navy/50 border border-glass-border rounded-lg px-4 py-2.5 text-sm text-white placeholder:text-muted/60 outline-none focus:border-gold/40 transition-colors';
const LABEL = 'block text-xs font-bold text-muted uppercase tracking-wider mb-1.5';

const INIT_FORM = { roundId: '', name: '', raceDate: '', distanceMeter: '', maxLanes: '' };

export function AdminRacesPage() {
  const [showModal, setShowModal] = useState(false);

  const [form, setForm] = useState(INIT_FORM);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  function set(field: string, value: string) {
    setForm(prev => ({ ...prev, [field]: value }));
  }

  async function handleCreate() {
    setError(''); setSuccess('');
    if (!form.roundId || !form.name || !form.raceDate || !form.distanceMeter || !form.maxLanes) {
      setError('Vui lòng điền đầy đủ tất cả các trường.');
      return;
    }
    setLoading(true);
    try {
      const data: any = await createRace({
        roundId: Number(form.roundId),
        name: form.name,
        raceDate: form.raceDate,
        distanceMeter: Number(form.distanceMeter),
        maxLanes: Number(form.maxLanes),
      });
      const newId = data?.result?.id ?? data?.result?.raceId;
      setSuccess(newId != null
        ? `Đã tạo cuộc đua thành công! ID = ${newId} — dùng ID này để kích hoạt chi trả / đặt cược.`
        : 'Tạo cuộc đua thành công!');
      setForm(INIT_FORM);
    } catch (err: unknown) {
      setError(parseApiError(err as Error));
    } finally {
      setLoading(false);
    }
  }

  function closeModal() {
    setShowModal(false);
    setError(''); setSuccess('');
    setForm(INIT_FORM);
  }

  return (
    <div className="min-h-screen text-body font-sans flex" style={{ backgroundColor: '#0b101e' }}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="gold" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title="Quản lý cuộc đua"
            subtitle="Lập lịch và theo dõi các vòng đua"
            imageUrl="/images/hero-admin.jpg"
            imagePosition="center center"
            actions={
              <button onClick={() => setShowModal(true)} className="btn-gold px-5 py-2.5 rounded-lg text-sm flex items-center gap-2 font-bold">
                <Plus size={16} /> Thêm cuộc đua
              </button>
            }
          />

          {/* Race List */}
          {/* TODO: BE chưa có API danh sách giải đấu & cuộc đua */}
          <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
            <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="text-4xl opacity-40 mb-3">🏁</div>
            <div className="text-muted text-sm">Chưa có cuộc đua</div>
            <div className="text-muted/60 text-xs mt-1">BE chưa có API danh sách — dùng nút "Thêm cuộc đua" để tạo mới</div>
          </div>

        </main>
      </div>

      {/* Add Race Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} className="glass-panel rounded-2xl p-8 w-full max-w-lg border border-gold/20 relative overflow-hidden">
            <div className="absolute top-0 left-8 right-8 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
            <div className="absolute -top-10 -right-10 w-40 h-40 rounded-full bg-gradient-to-br from-gold/10 to-transparent blur-[40px] pointer-events-none" />
            <div className="relative flex items-center gap-3 mb-6">
              <div className="w-8 h-8 rounded-lg bg-gold/10 border border-gold/20 flex items-center justify-center shrink-0">
                <Flag size={15} className="text-gold" />
              </div>
              <h2 className="text-xl font-serif text-white">Thêm cuộc đua mới</h2>
              <div className="flex-1 h-px bg-gradient-to-r from-gold/30 via-glass-border to-transparent" />
            </div>

            <div className="space-y-4">
              {/* TODO: cần BE bổ sung GET danh sách rounds để thay ô nhập tay bằng dropdown */}
              <div>
                <label className={LABEL}>Round ID * <span className="text-muted/50 normal-case font-normal">— nhập ID (BE chưa có API danh sách vòng đấu)</span></label>
                <input value={form.roundId} onChange={e => set('roundId', e.target.value)} type="number" min="1" placeholder="ID vòng đấu thuộc giải vừa tạo" className={INPUT} />
              </div>

              <div>
                <label className={LABEL}>Tên cuộc đua *</label>
                <input value={form.name} onChange={e => set('name', e.target.value)} placeholder="VD: Vòng 4 - Bán Kết" className={INPUT} />
              </div>

              <div>
                <label className={LABEL}>Ngày & giờ đua *</label>
                <input
                  type="datetime-local"
                  value={form.raceDate}
                  onChange={e => set('raceDate', e.target.value)}
                  className={INPUT}
                  style={{ colorScheme: 'dark' }}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className={LABEL}>Cự ly (m) *</label>
                  <input value={form.distanceMeter} onChange={e => set('distanceMeter', e.target.value)} type="number" min="100" placeholder="VD: 1600" className={INPUT} />
                </div>
                <div>
                  <label className={LABEL}>Số làn đua (Max Lanes) *</label>
                  <input value={form.maxLanes} onChange={e => set('maxLanes', e.target.value)} type="number" min="1" placeholder="VD: 12" className={INPUT} />
                </div>
              </div>

              {error && <div className="text-sm px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400">{error}</div>}
              {success && <div className="text-sm px-4 py-3 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">{success}</div>}
            </div>

            <div className="flex gap-3 mt-6">
              <button onClick={closeModal} className="flex-1 py-2.5 rounded-lg border border-glass-border text-muted hover:text-white hover:bg-white/5 text-sm font-medium transition-colors">Hủy</button>
              <button onClick={handleCreate} disabled={loading} className="flex-1 btn-gold py-2.5 rounded-lg text-sm font-bold disabled:opacity-60 disabled:cursor-not-allowed">
                {loading ? 'Đang tạo…' : 'Lưu cuộc đua'}
              </button>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
}
