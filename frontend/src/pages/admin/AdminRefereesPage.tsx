import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { AlertCircle, CheckCircle, UserPlus, Trash2, Calendar, ShieldAlert } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { getReferees, getRacesRefereeAssignments, assignReferee, removeReferee } from '../../api/adminService';

export function AdminRefereesPage() {
  const [referees, setReferees] = useState<any[]>([]);
  const [races, setRaces] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedRefereeForRace, setSelectedRefereeForRace] = useState<Record<number, string>>({});

  const fetchData = async () => {
    try {
      const [refereesRes, racesRes] = await Promise.all([
        getReferees(),
        getRacesRefereeAssignments()
      ]);

      const refereesData = refereesRes?.result ?? (Array.isArray(refereesRes) ? refereesRes : []);
      setReferees(refereesData);

      const racesData = racesRes?.result ?? (Array.isArray(racesRes) ? racesRes : []);
      setRaces(racesData);
    } catch (err) {
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleAssign = async (raceId: number) => {
    const refereeId = selectedRefereeForRace[raceId];
    if (!refereeId) return;
    try {
      await assignReferee(raceId, parseInt(refereeId, 10));
      await fetchData();
      setSelectedRefereeForRace(prev => ({ ...prev, [raceId]: '' }));
    } catch (err: any) {
      console.error('Error assigning referee:', err);
      alert(err.response?.data?.message || 'Có lỗi xảy ra khi phân công trọng tài');
    }
  };

  const handleRemove = async (raceId: number, refereeId: number) => {
    if (!window.confirm('Bạn có chắc chắn muốn hủy phân công trọng tài này cho cuộc đua?')) return;
    try {
      await removeReferee(raceId, refereeId);
      await fetchData();
    } catch (err: any) {
      console.error('Error removing referee:', err);
      alert(err.response?.data?.message || 'Có lỗi xảy ra khi hủy phân công trọng tài');
    }
  };

  // Filter races into assigned and unassigned
  const unassignedRaces = races.filter(r => !r.referees || r.referees.length === 0);
  const assignedRaces = races.filter(r => r.referees && r.referees.length > 0);

  return (
    <div className="min-h-screen text-body font-sans flex" style={{ backgroundColor: '#0b101e' }}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="gold" />
        <Topbar />
        <main className="max-w-[1600px] mx-auto px-8 py-6 space-y-6 relative z-10">

          <PageHero
            title="Referee Assignment"
            subtitle="Manage and assign licensed referees to scheduled tournament races"
            imageUrl="/images/hero-admin.jpg"
            imagePosition="center center"
          />

          {loading ? (
            <div className="text-center py-24 text-muted text-lg">Đang tải dữ liệu...</div>
          ) : (
            <div className="grid grid-cols-1 lg:grid-cols-[1fr_360px] gap-6">
              
              {/* Left Column: Races & Assignments */}
              <div className="space-y-6">
                
                {/* 1. Unassigned Races Section */}
                <motion.div 
                  initial={{ opacity: 0, y: 12 }} 
                  animate={{ opacity: 1, y: 0 }} 
                  className="glass-panel rounded-xl overflow-hidden"
                >
                  <div className="p-5 border-b border-glass-border flex items-center gap-2" style={{ backgroundColor: 'rgba(255, 255, 255, 0.02)' }}>
                    <AlertCircle size={18} className="text-yellow-400" />
                    <h2 className="text-lg font-serif text-white font-semibold">Chưa Phân Công Trọng Tài (Unassigned)</h2>
                    <span className="ml-auto px-2.5 py-0.5 rounded-full bg-yellow-500/10 text-yellow-400 text-xs font-bold border border-yellow-500/20">
                      {unassignedRaces.length}
                    </span>
                  </div>
                  
                  <div className="p-5 space-y-4">
                    {unassignedRaces.length === 0 ? (
                      <div className="p-12 text-center text-muted">
                        <CheckCircle size={32} className="mx-auto mb-2 text-emerald-400 opacity-60" />
                        Tất cả các trận đấu đã được phân công trọng tài.
                      </div>
                    ) : (
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {unassignedRaces.map((race) => (
                          <div 
                            key={race.raceId} 
                            className="p-4 rounded-xl bg-white/[0.02] border border-glass-border hover:border-gold/10 transition-all flex flex-col justify-between"
                          >
                            <div>
                              <div className="flex items-start justify-between gap-2 mb-2">
                                <span className="text-[10px] font-bold uppercase tracking-wider text-gold/80 bg-gold/10 px-2 py-0.5 rounded border border-gold/20">
                                  {race.roundName || 'Prefinal'}
                                </span>
                                <span className="text-xs text-muted flex items-center gap-1">
                                  <Calendar size={12} />
                                  {new Date(race.raceDate).toLocaleDateString('vi-VN')}
                                </span>
                              </div>
                              <h3 className="text-sm font-semibold text-white mb-1">{race.raceName}</h3>
                              <p className="text-xs text-muted mb-3 truncate">{race.tournamentName || 'Giải đấu'}</p>
                              <div className="text-xs text-muted/80 mb-4">
                                Cự ly: <span className="text-white font-mono">{race.distanceMeter}m</span>
                              </div>
                            </div>

                            <div className="pt-3 border-t border-glass-border flex gap-2">
                              <select
                                className="flex-1 bg-black/40 border border-glass-border text-white rounded px-2.5 py-1.5 text-xs focus:outline-none focus:border-gold/50"
                                value={selectedRefereeForRace[race.raceId] || ''}
                                onChange={(e) => setSelectedRefereeForRace(prev => ({ ...prev, [race.raceId]: e.target.value }))}
                              >
                                <option value="">-- Chọn trọng tài --</option>
                                {referees.map((ref) => (
                                  <option key={ref.refereeId} value={ref.refereeId}>
                                    {ref.fullName} ({ref.licenseNumber})
                                  </option>
                                ))}
                              </select>
                              <button
                                className="px-3 py-1.5 rounded bg-gold/25 hover:bg-gold/40 text-gold hover:text-white border border-gold/40 transition-all flex items-center gap-1 disabled:opacity-40 disabled:pointer-events-none"
                                disabled={!selectedRefereeForRace[race.raceId]}
                                onClick={() => handleAssign(race.raceId)}
                              >
                                <UserPlus size={14} />
                                <span className="text-xs font-semibold">Gán</span>
                              </button>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </motion.div>

                {/* 2. Already Assigned Section */}
                <motion.div 
                  initial={{ opacity: 0, y: 12 }} 
                  animate={{ opacity: 1, y: 0 }} 
                  transition={{ delay: 0.1 }} 
                  className="glass-panel rounded-xl overflow-hidden"
                >
                  <div className="p-5 border-b border-glass-border flex items-center gap-2" style={{ backgroundColor: 'rgba(255, 255, 255, 0.02)' }}>
                    <CheckCircle size={18} className="text-emerald-400" />
                    <h2 className="text-lg font-serif text-white font-semibold">Đã Phân Công Trọng Tài (Assigned)</h2>
                    <span className="ml-auto px-2.5 py-0.5 rounded-full bg-emerald-500/10 text-emerald-400 text-xs font-bold border border-emerald-500/20">
                      {assignedRaces.length}
                    </span>
                  </div>
                  
                  <div className="p-5 space-y-4">
                    {assignedRaces.length === 0 ? (
                      <div className="p-12 text-center text-muted">
                        <ShieldAlert size={32} className="mx-auto mb-2 text-yellow-400 opacity-60" />
                        Chưa có cuộc đua nào được gán trọng tài.
                      </div>
                    ) : (
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {assignedRaces.map((race) => (
                          <div 
                            key={race.raceId} 
                            className="p-4 rounded-xl bg-white/[0.02] border border-glass-border hover:border-gold/10 transition-all flex flex-col justify-between"
                          >
                            <div>
                              <div className="flex items-start justify-between gap-2 mb-2">
                                <span className="text-[10px] font-bold uppercase tracking-wider text-emerald-400 bg-emerald-500/10 px-2 py-0.5 rounded border border-emerald-500/20">
                                  {race.roundName || 'Prefinal'}
                                </span>
                                <span className="text-xs text-muted flex items-center gap-1">
                                  <Calendar size={12} />
                                  {new Date(race.raceDate).toLocaleDateString('vi-VN')}
                                </span>
                              </div>
                              <h3 className="text-sm font-semibold text-white mb-1">{race.raceName}</h3>
                              <p className="text-xs text-muted mb-3 truncate">{race.tournamentName || 'Giải đấu'}</p>
                              <div className="text-xs text-muted/80 mb-4">
                                Cự ly: <span className="text-white font-mono">{race.distanceMeter}m</span>
                              </div>
                            </div>

                            {/* Assigned Referees List */}
                            <div className="pt-3 border-t border-glass-border space-y-2">
                              <div className="text-xs font-semibold text-white/70 mb-1">Trọng tài phụ trách:</div>
                              <div className="space-y-1.5">
                                {race.referees.map((ref: any) => (
                                  <div 
                                    key={ref.refereeId} 
                                    className="flex items-center justify-between p-2 rounded bg-white/[0.01] border border-white/5 text-xs"
                                  >
                                    <div className="min-w-0 flex-1">
                                      <div className="font-semibold text-white truncate">{ref.fullName}</div>
                                      <div className="text-[10px] text-muted truncate">GP: {ref.licenseNumber}</div>
                                    </div>
                                    <button 
                                      className="p-1 rounded text-muted hover:text-red-400 hover:bg-red-500/10 transition-all"
                                      onClick={() => handleRemove(race.raceId, ref.refereeId)}
                                      title="Hủy phân công"
                                    >
                                      <Trash2 size={13} />
                                    </button>
                                  </div>
                                ))}
                              </div>

                              {/* Allow adding another referee */}
                              <div className="pt-3 flex gap-2">
                                <select
                                  className="flex-1 bg-black/40 border border-glass-border text-white rounded px-2.5 py-1.5 text-xs focus:outline-none focus:border-gold/50"
                                  value={selectedRefereeForRace[race.raceId] || ''}
                                  onChange={(e) => setSelectedRefereeForRace(prev => ({ ...prev, [race.raceId]: e.target.value }))}
                                >
                                  <option value="">-- Thêm trọng tài khác --</option>
                                  {referees
                                    .filter(ref => !race.referees.some((rRef: any) => rRef.refereeId === ref.refereeId))
                                    .map((ref) => (
                                      <option key={ref.refereeId} value={ref.refereeId}>
                                        {ref.fullName} ({ref.licenseNumber})
                                      </option>
                                    ))}
                                </select>
                                <button
                                  className="px-3 py-1.5 rounded bg-gold/25 hover:bg-gold/40 text-gold hover:text-white border border-gold/40 transition-all flex items-center gap-1 disabled:opacity-40 disabled:pointer-events-none"
                                  disabled={!selectedRefereeForRace[race.raceId]}
                                  onClick={() => handleAssign(race.raceId)}
                                >
                                  <UserPlus size={14} />
                                  <span className="text-xs font-semibold">Thêm</span>
                                </button>
                              </div>

                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </motion.div>

              </div>

              {/* Right Column: Referee Registry */}
              <motion.div 
                initial={{ opacity: 0, x: 16 }} 
                animate={{ opacity: 1, x: 0 }} 
                className="glass-panel rounded-xl p-5 h-fit"
              >
                <h2 className="text-lg font-serif text-white font-semibold mb-4">Danh Sách Trọng Tài</h2>
                {referees.length === 0 ? (
                  <div className="text-center py-12 text-muted">Không có trọng tài nào trong hệ thống.</div>
                ) : (
                  <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-1">
                    {referees.map((r) => (
                      <div 
                        key={r.refereeId} 
                        className="flex items-center gap-3 p-3 rounded-lg bg-white/[0.02] border border-glass-border hover:border-gold/20 transition-all"
                      >
                        <div className="w-9 h-9 rounded-full bg-gold/10 border border-gold/20 flex items-center justify-center shrink-0 text-gold text-sm font-bold font-serif">
                          {r.fullName ? r.fullName.charAt(0).toUpperCase() : 'R'}
                        </div>
                        <div className="min-w-0 flex-1">
                          <div className="text-sm font-semibold text-white truncate">{r.fullName}</div>
                          <div className="text-xs text-muted truncate">{r.email}</div>
                          <div className="text-[10px] text-gold/80 mt-0.5">
                            GP: {r.licenseNumber || 'N/A'} • {r.experienceYears} năm KN
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </motion.div>

            </div>
          )}

        </main>
      </div>
    </div>
  );
}
