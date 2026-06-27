import { useState, useEffect } from 'react';
import { Search, ShieldAlert, Heart } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { getRefereeDashboard, getHorseChecks } from '../../api/refereeService';

type Tab = 'all' | 'pending' | 'approved' | 'rejected';

interface AssignedRace {
  raceId: number;
  raceName: string;
  status: string;
}

interface HorseCheck {
  raceEntryId: number;
  horseId: number;
  horseName: string;
  ownerName: string;
  jockeyName: string;
  laneNo: number;
  medicalStatus: string;
  status: string;
}

export function RefereeHorseCheckPage() {
  const [tab, setTab] = useState<Tab>('all');
  const [search, setSearch] = useState('');
  const [races, setRaces] = useState<AssignedRace[]>([]);
  const [selectedRaceId, setSelectedRaceId] = useState<number | ''>('');
  const [horseChecks, setHorseChecks] = useState<HorseCheck[]>([]);
  const [loadingRaces, setLoadingRaces] = useState(true);
  const [loadingChecks, setLoadingChecks] = useState(false);
  const [error, setError] = useState('');

  // Fetch assigned races on mount
  useEffect(() => {
    getRefereeDashboard()
      .then(res => {
        if (res && res.result && res.result.assignedRaces) {
          const assigned = res.result.assignedRaces;
          setRaces(assigned);
          if (assigned.length > 0) {
            setSelectedRaceId(assigned[0].raceId);
          }
        }
        setLoadingRaces(false);
      })
      .catch(err => {
        console.error(err);
        setError('Không thể tải danh sách cuộc đua phân công');
        setLoadingRaces(false);
      });
  }, []);

  // Fetch horse checks when selected race changes
  useEffect(() => {
    if (!selectedRaceId) {
      setHorseChecks([]);
      return;
    }
    setLoadingChecks(true);
    getHorseChecks(selectedRaceId)
      .then(res => {
        if (res && res.result) {
          setHorseChecks(res.result);
        } else {
          setHorseChecks([]);
        }
        setLoadingChecks(false);
      })
      .catch(err => {
        console.error(err);
        setError('Không thể tải danh sách kiểm tra ngựa');
        setLoadingChecks(false);
      });
  }, [selectedRaceId]);

  // Filter logic
  const filteredChecks = horseChecks.filter(hc => {
    // Tab filter
    let tabMatch = true;
    if (tab === 'pending') tabMatch = hc.status?.toLowerCase() === 'pending';
    else if (tab === 'approved') tabMatch = hc.status?.toLowerCase() === 'confirmed' || hc.status?.toLowerCase() === 'checked' || hc.status?.toLowerCase() === 'ready' || hc.medicalStatus?.toLowerCase() === 'good' || hc.medicalStatus?.toLowerCase() === 'healthy';
    else if (tab === 'rejected') tabMatch = hc.status?.toLowerCase() === 'disqualified' || hc.medicalStatus?.toLowerCase() === 'unhealthy' || hc.medicalStatus?.toLowerCase() === 'sick';

    // Search filter
    const query = search.toLowerCase();
    const searchMatch = !search ||
      hc.horseName?.toLowerCase().includes(query) ||
      hc.ownerName?.toLowerCase().includes(query) ||
      hc.jockeyName?.toLowerCase().includes(query);

    return tabMatch && searchMatch;
  });

  return (
    <div className="min-h-screen text-body font-sans flex" style={{backgroundColor: '#0b101e'}}>
      <Sidebar />
      <div className="flex-1 relative min-w-0 overflow-y-auto">
        <PageAmbience accent="red" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title="Kiểm tra ngựa"
            subtitle="Xem xét và phê duyệt hồ sơ ngựa thi đấu theo cuộc đua"
            imageUrl="/images/hero-referee.jpg"
            imagePosition="right 52%"
          />

          {error && (
            <div className="p-4 bg-red-500/10 border border-red-500/20 text-red-400 rounded-lg text-sm">
              {error}
            </div>
          )}

          {/* Select Race Dropdown + Search */}
          <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between">
            <div className="flex items-center gap-3 w-full md:w-auto">
              <span className="text-sm text-muted font-bold shrink-0">Chọn cuộc đua:</span>
              {loadingRaces ? (
                <span className="text-xs text-muted">Đang tải cuộc đua...</span>
              ) : races.length === 0 ? (
                <span className="text-xs text-red-400">Không có cuộc đua nào được phân công</span>
              ) : (
                <select
                  value={selectedRaceId}
                  onChange={e => setSelectedRaceId(Number(e.target.value))}
                  className="bg-white/[0.04] border border-glass-border rounded-lg px-3 py-2 text-sm text-white focus:border-gold outline-none min-w-[200px]"
                >
                  {races.map(r => (
                    <option key={r.raceId} value={r.raceId} className="bg-[#0b101e]">
                      {r.raceName} (ID: {r.raceId})
                    </option>
                  ))}
                </select>
              )}
            </div>

            <div className="flex items-center gap-2 bg-white/[0.04] border border-glass-border rounded-lg px-3 py-2 w-full md:w-64">
              <Search size={14} className="text-muted shrink-0" />
              <input 
                value={search} 
                onChange={e => setSearch(e.target.value)} 
                placeholder="Tìm ngựa / chủ ngựa / nài..." 
                className="bg-transparent text-sm text-white placeholder:text-muted/60 outline-none w-full" 
              />
            </div>
          </div>

          {/* Tabs */}
          <div className="flex items-center gap-1 border-b border-glass-border">
            {([['all', 'Tất cả'], ['pending', 'Chờ kiểm tra'], ['approved', 'Đạt yêu cầu'], ['rejected', 'Không đạt']] as [Tab, string][]).map(([t, label]) => (
              <button 
                key={t} 
                onClick={() => setTab(t)}
                className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-all ${tab === t ? 'text-gold border-gold' : 'text-muted border-transparent hover:text-white'}`}
              >
                {label}
              </button>
            ))}
          </div>

          {/* Table */}
          {loadingChecks ? (
            <div className="glass-panel rounded-xl p-12 text-center text-muted">
              Đang tải danh sách ngựa...
            </div>
          ) : filteredChecks.length === 0 ? (
            <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
              <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
              <div className="text-4xl opacity-40 mb-3">🐴</div>
              <div className="text-muted text-sm">Chưa có dữ liệu kiểm tra ngựa nào của cuộc đua này</div>
            </div>
          ) : (
            <div className="glass-panel rounded-xl overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="border-b border-glass-border bg-white/[0.02] text-xs font-semibold text-muted uppercase tracking-wider">
                      <th className="px-6 py-4">Làn</th>
                      <th className="px-6 py-4">Tên ngựa</th>
                      <th className="px-6 py-4">Chủ sở hữu</th>
                      <th className="px-6 py-4">Nài ngựa</th>
                      <th className="px-6 py-4">Sức khỏe</th>
                      <th className="px-6 py-4">Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-glass-border/40 text-sm text-white">
                    {filteredChecks.map((hc) => (
                      <tr key={hc.raceEntryId} className="hover:bg-white/[0.01] transition-colors">
                        <td className="px-6 py-4 font-mono text-gold font-bold">Làn #{hc.laneNo}</td>
                        <td className="px-6 py-4 font-medium">{hc.horseName}</td>
                        <td className="px-6 py-4 text-muted">{hc.ownerName}</td>
                        <td className="px-6 py-4 text-muted">{hc.jockeyName}</td>
                        <td className="px-6 py-4">
                          <span className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded text-xs font-semibold ${
                            hc.medicalStatus?.toLowerCase() === 'good' || hc.medicalStatus?.toLowerCase() === 'healthy' ? 'bg-emerald-500/10 text-emerald-400' : 'bg-red-500/10 text-red-400'
                          }`}>
                            {hc.medicalStatus?.toLowerCase() === 'good' || hc.medicalStatus?.toLowerCase() === 'healthy' ? (
                              <><Heart size={11} /> Khỏe mạnh</>
                            ) : (
                              <><ShieldAlert size={11} /> Gặp sự cố / Yếu</>
                            )}
                          </span>
                        </td>
                        <td className="px-6 py-4">
                          <span className={`px-2 py-1 rounded text-xs font-semibold ${
                            hc.status?.toLowerCase() === 'pending' ? 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20' :
                            hc.status?.toLowerCase() === 'confirmed' || hc.status?.toLowerCase() === 'checked' || hc.status?.toLowerCase() === 'ready' ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' :
                            'bg-red-500/10 text-red-400 border border-red-500/20'
                          }`}>
                            {hc.status?.toLowerCase() === 'pending' ? 'Chờ kiểm tra' :
                             hc.status?.toLowerCase() === 'confirmed' || hc.status?.toLowerCase() === 'checked' || hc.status?.toLowerCase() === 'ready' ? 'Đã duyệt' : 'Bị loại'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

        </main>
      </div>
    </div>
  );
}
