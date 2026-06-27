import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Plus, Flag, UserCheck, ListOrdered, Trash2, Calendar, ChevronDown, ChevronUp, Trophy, Loader } from 'lucide-react';
import { Sidebar } from '../../components/layout/Sidebar';
import { Topbar } from '../../components/layout/Topbar';
import { PageHero } from '../../components/layout/PageHero';
import { PageAmbience } from '../../components/layout/PageAmbience';
import { createRace, deleteRace, createRaceEntry, assignReferee, getRaceReferees, removeReferee, generateTournamentRaces, getRegistrations, getReferees } from '../../api/adminService';
import { getRaceSchedule, getTournaments, getRaceEntries } from '../../api/publicService';
import { parseApiError } from '../../api/authService';


const INPUT = 'w-full bg-navy/50 border border-glass-border rounded-lg px-4 py-2.5 text-sm text-white placeholder:text-muted/60 outline-none focus:border-gold/40 transition-colors';
const LABEL = 'block text-xs font-bold text-muted uppercase tracking-wider mb-1.5';

const INIT_RACE = { roundId: '', name: '', raceDate: '', distanceMeter: '1200', maxLanes: '12' };
const INIT_ENTRY = { raceId: '', registrationId: '', laneNo: '' };
const INIT_REF = { tournamentId: '', raceId: '', refereeId: '' };

type Modal = 'none' | 'race' | 'entry' | 'referee';

const fixMojibake = (str: string): string => {
  if (!str) return '';
  try {
    return decodeURIComponent(escape(str));
  } catch (e) {
    return str;
  }
};

export function AdminRacesPage() {
  const [modal, setModal] = useState<Modal>('none');

  // List Tournaments and Races
  const [tournamentsList, setTournamentsList] = useState<any[]>([]);
  const [racesList, setRacesList] = useState<any[]>([]);
  const [registrationsList, setRegistrationsList] = useState<any[]>([]);
  const [loadingData, setLoadingData] = useState(false);
  const [fetchError, setFetchError] = useState('');

  // Expand race details
  const [expandedRaceId, setExpandedRaceId] = useState<number | null>(null);
  const [expandedRaceDetails, setExpandedRaceDetails] = useState<{
    entries: any[];
    referees: any[];
    loading: boolean;
  }>({ entries: [], referees: [], loading: false });

  // Generate races loading status
  const [generatingForTournament, setGeneratingForTournament] = useState<number | null>(null);

  // Create Race
  const [raceForm, setRaceForm] = useState(INIT_RACE);
  const [raceLoading, setRaceLoading] = useState(false);
  const [raceError, setRaceError] = useState('');
  const [raceSuccess, setRaceSuccess] = useState('');

  // Race Entry
  const [entryForm, setEntryForm] = useState(INIT_ENTRY);
  const [entryLoading, setEntryLoading] = useState(false);
  const [entryError, setEntryError] = useState('');
  const [entrySuccess, setEntrySuccess] = useState('');

  // Referee
  const [refForm, setRefForm] = useState(INIT_REF);
  const [refLoading, setRefLoading] = useState(false);
  const [refError, setRefError] = useState('');
  const [refSuccess, setRefSuccess] = useState('');
  const [referees, setReferees] = useState<any[]>([]);
  const [refereeOptions, setRefereeOptions] = useState<any[]>([]);
  const [refViewId, setRefViewId] = useState('');
  const [refViewLoading, setRefViewLoading] = useState(false);

  function setR(field: string, v: string) { setRaceForm(p => ({ ...p, [field]: v })); }
  function setE(field: string, v: string) { setEntryForm(p => ({ ...p, [field]: v })); }
  function setF(field: string, v: string) { setRefForm(p => ({ ...p, [field]: v })); }

  async function loadAllData() {
    setLoadingData(true);
    setFetchError('');
    try {
      const [tRes, rRes, regRes, refRes] = await Promise.all([
        getTournaments(),
        getRaceSchedule(),
        getRegistrations(),
        getReferees()
      ]);
      
      const tournaments = tRes?.result ?? (Array.isArray(tRes) ? tRes : []);
      const races = rRes?.result ?? (Array.isArray(rRes) ? rRes : []);
      const registrations = regRes?.result ?? (Array.isArray(regRes) ? regRes : []);
      const fetchedReferees = refRes?.result ?? (Array.isArray(refRes) ? refRes : []);
      
      setTournamentsList(tournaments.map((t: any) => ({
        ...t,
        name: fixMojibake(t.name)
      })));
      
      setRacesList(races.map((r: any) => ({
        ...r,
        name: fixMojibake(r.name)
      })));

      setRegistrationsList(registrations.map((r: any) => ({
        ...r,
        horseName: fixMojibake(r.horseName),
        ownerName: fixMojibake(r.ownerName),
        tournamentName: fixMojibake(r.tournamentName)
      })));

      setRefereeOptions(fetchedReferees.map((r: any) => ({
        ...r,
        fullName: fixMojibake(r.fullName ?? r.FullName ?? ''),
        email: r.email ?? r.Email,
        licenseNumber: r.licenseNumber ?? r.LicenseNumber,
        status: r.status ?? r.Status,
        refereeId: r.refereeId ?? r.RefereeId ?? r.id
      })));
    } catch (err: any) {
      setFetchError(parseApiError(err));
    } finally {
      setLoadingData(false);
    }
  }

  useEffect(() => {
    loadAllData();
  }, []);

  async function toggleExpandRace(raceId: number) {
    if (expandedRaceId === raceId) {
      setExpandedRaceId(null);
      return;
    }
    setExpandedRaceId(raceId);
    setExpandedRaceDetails({ entries: [], referees: [], loading: true });
    try {
      const [entriesRes, refereesRes] = await Promise.all([
        getRaceEntries(raceId),
        getRaceReferees(raceId)
      ]);
      
      const rawEntries = entriesRes?.result ?? (Array.isArray(entriesRes) ? entriesRes : []);
      const rawReferees = refereesRes?.result ?? (Array.isArray(refereesRes) ? refereesRes : []);

      const cleanEntries = rawEntries.map((e: any) => ({
        ...e,
        horseName: fixMojibake(e.horseName),
        ownerName: fixMojibake(e.ownerName),
        jockeyName: fixMojibake(e.jockeyName)
      }));

      const cleanReferees = rawReferees.map((r: any) => ({
        ...r,
        fullName: fixMojibake(r.fullName),
        refereeName: fixMojibake(r.refereeName)
      }));

      setExpandedRaceDetails({
        entries: cleanEntries,
        referees: cleanReferees,
        loading: false
      });
    } catch (err) {
      console.error(err);
      setExpandedRaceDetails({ entries: [], referees: [], loading: false });
    }
  }

  async function handleGenerateRaces(tournamentId: number) {
    setGeneratingForTournament(tournamentId);
    try {
      await generateTournamentRaces(tournamentId);
      alert('Đã sinh cuộc đua thành công!');
      await loadAllData();
    } catch (err: any) {
      alert('Lỗi khi sinh cuộc đua: ' + parseApiError(err));
    } finally {
      setGeneratingForTournament(null);
    }
  }

  function openRaceModal(roundId?: number) {
    setRaceForm({
      roundId: roundId ? String(roundId) : '',
      name: '',
      raceDate: '',
      distanceMeter: '1200',
      maxLanes: '12'
    });
    setModal('race');
  }

  function openEntryModal(raceId?: number) {
    setEntryForm({
      raceId: raceId ? String(raceId) : '',
      registrationId: '',
      laneNo: ''
    });
    setModal('entry');
  }

  function openRefereeModal(raceId?: number) {
    const race = raceId ? racesList.find(r => Number(r.raceId) === Number(raceId)) : null;
    setRefForm({
      tournamentId: race?.tournamentId ? String(race.tournamentId) : '',
      raceId: raceId ? String(raceId) : '',
      refereeId: ''
    });
    setModal('referee');
  }

  function closeModal() {
    setModal('none');
    setRaceError(''); setRaceSuccess(''); setRaceForm(INIT_RACE);
    setEntryError(''); setEntrySuccess(''); setEntryForm(INIT_ENTRY);
    setRefError(''); setRefSuccess(''); setRefForm(INIT_REF);
    setReferees([]); setRefViewId('');
  }

  async function handleCreateRace() {
    setRaceError(''); setRaceSuccess('');
    if (!raceForm.roundId || !raceForm.name || !raceForm.raceDate || !raceForm.distanceMeter || !raceForm.maxLanes) {
      setRaceError('Vui lòng điền đầy đủ tất cả các trường.');
      return;
    }
    setRaceLoading(true);
    try {
      const data: any = await createRace({
        roundId: Number(raceForm.roundId),
        name: raceForm.name,
        raceDate: raceForm.raceDate,
        distanceMeter: Number(raceForm.distanceMeter),
        maxLanes: Number(raceForm.maxLanes),
      });
      const newId = data?.result?.id ?? data?.result?.raceId ?? data?.raceId;
      setRaceSuccess(newId != null
        ? `Đã tạo cuộc đua thành công! ID = ${newId}`
        : 'Tạo cuộc đua thành công!');
      setRaceForm(INIT_RACE);
      await loadAllData();
    } catch (err: unknown) {
      setRaceError(parseApiError(err as Error));
    } finally {
      setRaceLoading(false);
    }
  }

  async function handleCreateEntry() {
    setEntryError(''); setEntrySuccess('');
    if (!entryForm.raceId || !entryForm.registrationId || !entryForm.laneNo) {
      setEntryError('Vui lòng chọn cuộc đua, chọn ngựa đã được duyệt và nhập số làn.');
      return;
    }
    setEntryLoading(true);
    try {
      await createRaceEntry(Number(entryForm.raceId), {
        registrationId: Number(entryForm.registrationId),
        laneNo: Number(entryForm.laneNo),
      });
      setEntrySuccess('Đã ghép ngựa vào làn thành công!');
      setEntryForm(INIT_ENTRY);
      await loadAllData();
      if (expandedRaceId === Number(entryForm.raceId)) {
        await toggleExpandRace(Number(entryForm.raceId));
      }
    } catch (err: unknown) {
      setEntryError(parseApiError(err as Error));
    } finally {
      setEntryLoading(false);
    }
  }

  async function handleAssignReferee() {
    setRefError(''); setRefSuccess('');
    if (!refForm.tournamentId || !refForm.raceId || !refForm.refereeId) {
      setRefError('Vui lòng chọn giải đấu, cuộc đua và trọng tài.');
      return;
    }
    setRefLoading(true);
    try {
      await assignReferee(Number(refForm.raceId), Number(refForm.refereeId));
      setRefSuccess('Đã phân công trọng tài thành công!');
      setRefForm(p => ({ ...p, refereeId: '' }));
      if (refViewId === refForm.raceId) await handleViewReferees(refForm.raceId);
      if (expandedRaceId === Number(refForm.raceId)) {
        await toggleExpandRace(Number(refForm.raceId));
      }
    } catch (err: unknown) {
      setRefError(parseApiError(err as Error));
    } finally {
      setRefLoading(false);
    }
  }

  async function handleViewReferees(raceId: string) {
    if (!raceId) return;
    setRefViewId(raceId);
    setRefViewLoading(true);
    try {
      const data: any = await getRaceReferees(Number(raceId));
      setReferees(data?.result ?? (Array.isArray(data) ? data : []));
    } catch {
      setReferees([]);
    } finally {
      setRefViewLoading(false);
    }
  }

  async function handleRemoveReferee(raceId: string, refereeId: number) {
    try {
      await removeReferee(Number(raceId), refereeId);
      setReferees(prev => prev.filter(r => (r.id ?? r.refereeId) !== refereeId));
      if (expandedRaceId === Number(raceId)) {
        await toggleExpandRace(Number(raceId));
      }
    } catch (err: unknown) {
      alert(parseApiError(err as Error));
    }
  }

  async function handleDeleteRace(raceId: number, raceName?: string) {
    const label = raceName ? `"${raceName}"` : `#${raceId}`;
    if (!window.confirm(`Xóa cuộc đua ${label}? Dữ liệu làn, kết quả, trọng tài, vi phạm, cược và dự đoán liên quan cũng sẽ bị xóa.`)) {
      return;
    }

    try {
      await deleteRace(raceId);
      if (expandedRaceId === raceId) {
        setExpandedRaceId(null);
      }
      await loadAllData();
    } catch (err: unknown) {
      alert(parseApiError(err as Error));
    }
  }

  const groupedTournaments = tournamentsList.map(t => {
    const tRaces = racesList.filter(r => r.tournamentId === t.tournamentId);
    
    const rounds = (t.rounds ?? []).map((r: any) => {
      const rRaces = tRaces.filter(race => race.roundId === r.roundId);
      return {
        ...r,
        races: rRaces
      };
    });

    const prefinalRound = rounds.find((r: any) => r.roundNumber === 1);
    const finalRound = rounds.find((r: any) => r.roundNumber === 2);
    const hasPrefinalRaces = Boolean(prefinalRound && prefinalRound.races.length > 0);
    const prefinalFinished = Boolean(
      prefinalRound &&
      prefinalRound.races.length > 0 &&
      prefinalRound.races.every((r: any) => r.status === 'Finished')
    );
    
    const canGenerateFinal = 
      prefinalFinished &&
      finalRound &&
      finalRound.races.length === 0;
    const canGeneratePre = !prefinalFinished;
    const waitingLabel = hasPrefinalRaces && !prefinalFinished
      ? 'Chờ hoàn thành Pre'
      : hasPrefinalRaces && prefinalFinished && (!finalRound || finalRound.races.length === 0)
        ? 'Thiếu Final Race'
        : '';

    return {
      ...t,
      rounds,
      canGeneratePre,
      canGenerateFinal,
      waitingLabel
    };
  });

  const selectedEntryRace = racesList.find(r => String(r.raceId) === String(entryForm.raceId));
  const approvedRegistrationsForEntryRace = registrationsList.filter((reg: any) => {
    const status = String(reg.status ?? '').toLowerCase();
    const registrationTournamentId = reg.tournamentId ?? reg.TournamentId;
    return status === 'approved' &&
      (!selectedEntryRace || Number(registrationTournamentId) === Number(selectedEntryRace.tournamentId));
  });
  const racesForSelectedRefTournament = racesList.filter((race: any) =>
    refForm.tournamentId && Number(race.tournamentId) === Number(refForm.tournamentId)
  );
  const activeRefereeOptions = refereeOptions.filter((ref: any) =>
    String(ref.status ?? '').toLowerCase() === 'active'
  );
  const visibleRefereeOptions = activeRefereeOptions.length > 0 ? activeRefereeOptions : refereeOptions;

  return (
    <div className="min-h-screen text-body font-sans flex" style={{ backgroundColor: '#0b101e' }}>
      <Sidebar />
      <div className="flex-1 min-w-0 overflow-y-auto relative">
        <PageAmbience accent="gold" />
        <Topbar />
        <main className="relative z-10 max-w-[1600px] mx-auto px-8 py-6 space-y-6">

          <PageHero
            title="Quản lý cuộc đua"
            subtitle="Lập lịch, ghép ngựa và phân công trọng tài"
            imageUrl="/images/hero-admin.jpg"
            imagePosition="center center"
            actions={
              <div className="flex items-center gap-3">
                <button onClick={() => openRaceModal()} className="btn-gold px-5 py-2.5 rounded-lg text-sm flex items-center gap-2 font-bold">
                  <Plus size={16} /> Thêm cuộc đua
                </button>
                <button onClick={() => openEntryModal()} className="px-5 py-2.5 rounded-lg text-sm flex items-center gap-2 font-bold text-blue-400 border border-blue-500/30 bg-blue-500/10 hover:bg-blue-500/20 transition-colors">
                  <ListOrdered size={16} /> Ghép ngựa vào làn
                </button>
                <button onClick={() => openRefereeModal()} className="px-5 py-2.5 rounded-lg text-sm flex items-center gap-2 font-bold text-cyan-400 border border-cyan-500/30 bg-cyan-500/10 hover:bg-cyan-500/20 transition-colors">
                  <UserCheck size={16} /> Phân công trọng tài
                </button>
              </div>
            }
          />

          {loadingData && (
            <div className="flex items-center justify-center py-20 gap-3 text-gold">
              <Loader className="animate-spin" size={24} />
              <span>Đang tải danh sách giải đấu và cuộc đua...</span>
            </div>
          )}

          {fetchError && (
            <div className="px-6 py-4 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
              Lỗi tải dữ liệu: {fetchError}
            </div>
          )}

          {!loadingData && !fetchError && groupedTournaments.length === 0 && (
            <div className="glass-panel rounded-xl p-12 text-center relative overflow-hidden">
              <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/40 to-transparent pointer-events-none" />
              <div className="text-4xl opacity-40 mb-3">🏆</div>
              <div className="text-muted text-sm">Chưa có giải đấu nào</div>
              <div className="text-muted/60 text-xs mt-1">Vui lòng tạo giải đấu tại trang Quản lý giải đấu trước.</div>
            </div>
          )}

          {!loadingData && !fetchError && groupedTournaments.length > 0 && (
            <div className="space-y-8">
              {groupedTournaments.map(t => (
                <div key={t.tournamentId} className="glass-panel rounded-xl p-6 relative overflow-hidden space-y-6 animate-fade-in">
                  <div className="absolute top-0 left-6 right-6 h-px bg-gradient-to-r from-transparent via-gold/30 to-transparent pointer-events-none" />
                  
                  {/* Tournament Header */}
                  <div className="flex flex-wrap items-center justify-between gap-4 border-b border-glass-border pb-4">
                    <div>
                      <div className="flex items-center gap-2">
                        <Trophy size={18} className="text-gold" />
                        <h2 className="text-lg font-serif text-white font-bold">{t.name}</h2>
                        <span className={`text-[10px] px-2 py-0.5 rounded-full border ${
                          t.status === 'Active' ? 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20' :
                          t.status === 'Upcoming' ? 'text-blue-400 bg-blue-500/10 border-blue-500/20' :
                          'text-muted bg-white/5 border-glass-border'
                        }`}>
                          {t.status === 'Active' ? 'Đang diễn ra' : t.status === 'Upcoming' ? 'Sắp diễn ra' : 'Đã kết thúc'}
                        </span>
                      </div>
                      {t.startDate && t.endDate && (
                        <p className="text-xs text-muted/80 mt-1">
                          Thời gian: {new Date(t.startDate).toLocaleDateString()} - {new Date(t.endDate).toLocaleDateString()}
                        </p>
                      )}
                    </div>

                    <div className="flex items-center gap-3">
                      {/* Prefinal generation */}
                      {t.canGeneratePre && (
                        <button
                          onClick={() => handleGenerateRaces(t.tournamentId)}
                          disabled={generatingForTournament === t.tournamentId}
                          className="px-4 py-2 bg-blue-500/20 hover:bg-blue-500/30 text-blue-400 border border-blue-500/30 text-xs font-bold rounded-lg transition-colors flex items-center gap-1.5"
                        >
                          {generatingForTournament === t.tournamentId ? (
                            <>
                              <Loader size={12} className="animate-spin" />
                              Đang tạo...
                            </>
                          ) : (
                            'Auto xếp làn đua'
                          )}
                        </button>
                      )}

                      {/* Final generation */}
                      {t.canGenerateFinal && (
                        <button
                          onClick={() => handleGenerateRaces(t.tournamentId)}
                          disabled={generatingForTournament === t.tournamentId}
                          className="px-4 py-2 bg-gold/20 hover:bg-gold/30 text-gold border border-gold/30 text-xs font-bold rounded-lg transition-colors flex items-center gap-1.5 animate-pulse"
                        >
                          {generatingForTournament === t.tournamentId ? (
                            <>
                              <Loader size={12} className="animate-spin" />
                              Đang tạo...
                            </>
                          ) : (
                            'Auto xếp Final (Top 12)'
                          )}
                        </button>
                      )}
                      {t.rounds?.[0]?.races?.length > 0 && !t.canGenerateFinal && t.waitingLabel && (
                        <button
                          disabled
                          className="px-4 py-2 bg-white/[0.04] text-muted border border-glass-border text-xs font-bold rounded-lg cursor-not-allowed"
                        >
                          {t.waitingLabel}
                        </button>
                      )}
                    </div>
                  </div>

                  {/* Rounds */}
                  <div className="grid grid-cols-1 gap-6">
                    {t.rounds.map((r: any) => (
                      <div key={r.roundId} className="space-y-3 bg-navy/20 p-4 rounded-xl border border-glass-border/40">
                        <div className="flex items-center justify-between border-b border-glass-border/30 pb-2">
                          <h3 className="text-sm font-bold text-champagne uppercase tracking-wider flex items-center gap-2">
                            <span>{r.name}</span>
                            <span className="text-[10px] text-muted normal-case font-normal">
                              ({r.roundNumber === 1 ? 'Prefinal Round - Vòng loại' : 'Final Round - Chung kết'})
                            </span>
                          </h3>
                          <button
                            onClick={() => openRaceModal(r.roundId)}
                            className="text-[11px] text-gold hover:underline flex items-center gap-1 font-semibold"
                          >
                            <Plus size={12} /> Thêm cuộc đua thủ công
                          </button>
                        </div>

                        {r.races.length === 0 ? (
                          <div className="text-xs text-muted/50 italic py-4">
                            Chưa có cuộc đua nào được lập lịch. Dùng nút chia bảng tự động hoặc thêm thủ công.
                          </div>
                        ) : (
                          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                            {r.races.map((race: any) => {
                              const isExpanded = expandedRaceId === race.raceId;
                              return (
                                <div
                                  key={race.raceId}
                                  className={`bg-navy/40 border rounded-xl overflow-hidden transition-all duration-300 ${
                                    isExpanded ? 'border-gold/30 shadow-lg shadow-gold/5 bg-navy/60' : 'border-glass-border hover:border-white/10'
                                  }`}
                                >
                                  <div className="p-4 space-y-3">
                                    <div className="flex items-start justify-between gap-2">
                                      <div>
                                        <h4 className="text-sm font-bold text-white flex items-center gap-1.5">
                                          🏁 {race.name}
                                        </h4>
                                        <span className={`text-[9px] font-bold px-1.5 py-0.5 rounded uppercase ${
                                          race.status === 'Finished' ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' :
                                          race.status === 'Live' ? 'bg-red-500/10 text-red-400 border border-red-500/20 animate-pulse' :
                                          'bg-blue-500/10 text-blue-400 border border-blue-500/20'
                                        }`}>
                                          {race.status === 'Finished' ? 'Đã hoàn thành' : race.status === 'Live' ? 'Đang diễn ra' : 'Đã lên lịch'}
                                        </span>
                                      </div>
                                      <div className="text-right">
                                        <div className="text-[11px] text-muted flex items-center gap-1 justify-end">
                                          <Calendar size={11} />
                                          {new Date(race.raceDate).toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit' })}
                                        </div>
                                        <div className="text-[10px] text-muted/80 mt-0.5">
                                          Cự ly: {race.distanceMeter}m | Làn tối đa: {race.maxLanes}
                                        </div>
                                      </div>
                                    </div>

                                    <div className="flex items-center justify-between gap-2 pt-2 border-t border-glass-border/30">
                                      <button
                                        onClick={() => toggleExpandRace(race.raceId)}
                                        className="text-[11px] text-muted hover:text-white flex items-center gap-1 transition-colors"
                                      >
                                        {isExpanded ? (
                                          <>
                                            Thu gọn <ChevronUp size={12} />
                                          </>
                                        ) : (
                                          <>
                                            Chi tiết làn & trọng tài <ChevronDown size={12} />
                                          </>
                                        )}
                                      </button>

                                      <div className="flex items-center gap-2">
                                        <button
                                          onClick={() => openEntryModal(race.raceId)}
                                          title="Ghép ngựa vào làn"
                                          className="p-1.5 rounded-lg text-blue-400 hover:bg-blue-500/10 border border-transparent hover:border-blue-500/20 transition-colors"
                                        >
                                          <ListOrdered size={13} />
                                        </button>
                                        <button
                                          onClick={() => openRefereeModal(race.raceId)}
                                          title="Phân công trọng tài"
                                          className="p-1.5 rounded-lg text-cyan-400 hover:bg-cyan-500/10 border border-transparent hover:border-cyan-500/20 transition-colors"
                                        >
                                          <UserCheck size={13} />
                                        </button>
                                        <button
                                          onClick={() => handleDeleteRace(race.raceId, race.name)}
                                          title="Xóa cuộc đua"
                                          className="p-1.5 rounded-lg text-red-400 hover:bg-red-500/10 border border-transparent hover:border-red-500/20 transition-colors"
                                        >
                                          <Trash2 size={13} />
                                        </button>
                                      </div>
                                    </div>
                                  </div>

                                  {isExpanded && (
                                    <div className="border-t border-glass-border/30 bg-navy/60 p-4 space-y-4 text-xs">
                                      {expandedRaceDetails.loading ? (
                                        <div className="flex items-center justify-center py-4 gap-2 text-muted">
                                          <Loader size={14} className="animate-spin" />
                                          Đang tải chi tiết...
                                        </div>
                                      ) : (
                                        <>
                                          {/* Referees */}
                                          <div className="space-y-1.5">
                                            <div className="font-bold text-[10px] text-muted uppercase tracking-wider">Trọng tài giám sát:</div>
                                            {expandedRaceDetails.referees.length === 0 ? (
                                              <div className="text-muted/60 italic">Chưa phân công trọng tài</div>
                                            ) : (
                                              <div className="flex flex-wrap gap-1.5">
                                                {expandedRaceDetails.referees.map((ref: any) => (
                                                  <div key={ref.id ?? ref.refereeId} className="flex items-center gap-1.5 px-2 py-1 rounded bg-white/5 border border-glass-border text-white text-[11px]">
                                                     <span>👤 {ref.refereeName ?? ref.fullName ?? ref.name ?? `Trọng tài #${ref.refereeId ?? ref.id}`}</span>
                                                    <button
                                                      onClick={() => handleRemoveReferee(String(race.raceId), ref.id ?? ref.refereeId)}
                                                      className="text-red-400 hover:text-red-300 font-bold ml-1 text-xs"
                                                      title="Hủy phân công"
                                                    >
                                                      ×
                                                    </button>
                                                  </div>
                                                ))}
                                              </div>
                                            )}
                                          </div>

                                          {/* Lanes */}
                                          <div className="space-y-1.5">
                                            <div className="font-bold text-[10px] text-muted uppercase tracking-wider">Danh sách ngựa & làn đua:</div>
                                            {expandedRaceDetails.entries.length === 0 ? (
                                              <div className="text-muted/60 italic">Chưa ghép ngựa vào làn</div>
                                            ) : (
                                              <div className="space-y-1 bg-black/20 p-2 rounded-lg border border-glass-border/40 max-h-[200px] overflow-y-auto">
                                                {expandedRaceDetails.entries.map((entry: any) => (
                                                  <div key={entry.raceEntryId} className="flex items-center justify-between text-white border-b border-white/5 last:border-0 py-1.5">
                                                    <div className="flex items-center gap-2">
                                                      <span className="font-bold text-gold bg-gold/10 px-1.5 py-0.5 rounded text-[10px] w-6 text-center">
                                                        L{entry.laneNo}
                                                      </span>
                                                      <span className="font-medium text-[13px]">🐎 {entry.horseName || `Ngựa #${entry.horseId}`}</span>
                                                    </div>
                                                    <span className="text-[10px] text-muted">
                                                      Kỵ sĩ: {entry.jockeyName || 'Chưa phân công'}
                                                    </span>
                                                  </div>
                                                ))}
                                              </div>
                                            )}
                                          </div>
                                        </>
                                      )}
                                    </div>
                                  )}
                                </div>
                              );
                            })}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}

        </main>
      </div>

      {/* ── Modal: Thêm cuộc đua ── */}
      {modal === 'race' && (
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
              <div>
                <label className={LABEL}>Vòng đấu của giải đấu *</label>
                <select
                  value={raceForm.roundId}
                  onChange={e => setR('roundId', e.target.value)}
                  className={`${INPUT} bg-navy`}
                  style={{ colorScheme: 'dark' }}
                >
                  <option value="">-- Chọn vòng đấu --</option>
                  {tournamentsList.map(t => (
                    <optgroup key={t.tournamentId} label={t.name}>
                      {(t.rounds ?? []).map((r: any) => (
                        <option key={r.roundId} value={r.roundId}>
                          {r.name} (Round {r.roundNumber})
                        </option>
                      ))}
                    </optgroup>
                  ))}
                </select>
              </div>
              <div>
                <label className={LABEL}>Tên cuộc đua *</label>
                <input value={raceForm.name} onChange={e => setR('name', e.target.value)} placeholder="VD: Race 1 (Prefinal)" className={INPUT} />
              </div>
              <div>
                <label className={LABEL}>Ngày & giờ đua *</label>
                <input type="datetime-local" value={raceForm.raceDate} onChange={e => setR('raceDate', e.target.value)} className={INPUT} style={{ colorScheme: 'dark' }} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className={LABEL}>Cự ly (m) *</label>
                  <input value={raceForm.distanceMeter} onChange={e => setR('distanceMeter', e.target.value)} type="number" min="100" placeholder="VD: 1200" className={INPUT} />
                </div>
                <div>
                  <label className={LABEL}>Số làn đua *</label>
                  <input value={raceForm.maxLanes} onChange={e => setR('maxLanes', e.target.value)} type="number" min="1" placeholder="VD: 12" className={INPUT} />
                </div>
              </div>
              {raceError && <div className="text-sm px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400">{raceError}</div>}
              {raceSuccess && <div className="text-sm px-4 py-3 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">{raceSuccess}</div>}
            </div>

            <div className="flex gap-3 mt-6">
              <button onClick={closeModal} className="flex-1 py-2.5 rounded-lg border border-glass-border text-muted hover:text-white hover:bg-white/5 text-sm font-medium transition-colors">Hủy</button>
              <button onClick={handleCreateRace} disabled={raceLoading} className="flex-1 btn-gold py-2.5 rounded-lg text-sm font-bold disabled:opacity-60 disabled:cursor-not-allowed">
                {raceLoading ? 'Đang tạo…' : 'Lưu cuộc đua'}
              </button>
            </div>
          </motion.div>
        </div>
      )}

      {/* ── Modal: Ghép ngựa vào làn ── */}
      {modal === 'entry' && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} className="glass-panel rounded-2xl p-8 w-full max-w-md border border-blue-500/20 relative overflow-hidden">
            <div className="absolute top-0 left-8 right-8 h-px bg-gradient-to-r from-transparent via-blue-400/40 to-transparent pointer-events-none" />
            <div className="absolute -top-10 -right-10 w-40 h-40 rounded-full bg-gradient-to-br from-blue-500/10 to-transparent blur-[40px] pointer-events-none" />
            <div className="relative flex items-center gap-3 mb-6">
              <div className="w-8 h-8 rounded-lg bg-blue-500/10 border border-blue-500/20 flex items-center justify-center shrink-0">
                <ListOrdered size={15} className="text-blue-400" />
              </div>
              <h2 className="text-xl font-serif text-white">Ghép ngựa vào làn đua</h2>
              <div className="flex-1 h-px bg-gradient-to-r from-blue-400/30 via-glass-border to-transparent" />
            </div>

            <div className="space-y-4">
              <div>
                <label className={LABEL}>Chọn cuộc đua *</label>
                <select
                  value={entryForm.raceId}
                  onChange={e => setEntryForm(p => ({ ...p, raceId: e.target.value, registrationId: '' }))}
                  className={`${INPUT} bg-navy`}
                  style={{ colorScheme: 'dark' }}
                >
                  <option value="">-- Chọn cuộc đua --</option>
                  {racesList.map((race: any) => (
                    <option key={race.raceId} value={race.raceId}>
                      {race.name} #{race.raceId} - {race.tournamentName ?? `Tournament #${race.tournamentId}`}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className={LABEL}>Chọn ngựa đã đăng ký và được duyệt *</label>
                <select
                  value={entryForm.registrationId}
                  onChange={e => setE('registrationId', e.target.value)}
                  className={`${INPUT} bg-navy`}
                  style={{ colorScheme: 'dark' }}
                  disabled={!entryForm.raceId}
                >
                  <option value="">{entryForm.raceId ? '-- Chọn ngựa đã duyệt --' : '-- Chọn cuộc đua trước --'}</option>
                  {approvedRegistrationsForEntryRace.map((reg: any) => (
                    <option key={reg.registrationId ?? reg.id} value={reg.registrationId ?? reg.id}>
                      {reg.horseName ?? `Ngựa #${reg.horseId}`} - {reg.ownerName ?? 'Không rõ chủ'} (Reg #{reg.registrationId ?? reg.id})
                    </option>
                  ))}
                </select>
                {entryForm.raceId && approvedRegistrationsForEntryRace.length === 0 && (
                  <div className="text-[11px] text-yellow-400 mt-1.5">
                    Chưa có ngựa nào đã được duyệt đăng ký cho giải đấu của cuộc đua này.
                  </div>
                )}
              </div>
              <div>
                <label className={LABEL}>Số làn đua *</label>
                <input value={entryForm.laneNo} onChange={e => setE('laneNo', e.target.value)} type="number" min="1" placeholder="VD: 3" className={INPUT} />
              </div>
              {entryError && <div className="text-sm px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400">{entryError}</div>}
              {entrySuccess && <div className="text-sm px-4 py-3 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">{entrySuccess}</div>}
            </div>

            <div className="flex gap-3 mt-6">
              <button onClick={closeModal} className="flex-1 py-2.5 rounded-lg border border-glass-border text-muted hover:text-white hover:bg-white/5 text-sm font-medium transition-colors">Đóng</button>
              <button onClick={handleCreateEntry} disabled={entryLoading} className="flex-1 py-2.5 rounded-lg bg-blue-500/20 text-blue-400 border border-blue-500/30 hover:bg-blue-500/30 text-sm font-bold disabled:opacity-60 disabled:cursor-not-allowed transition-colors">
                {entryLoading ? 'Đang ghép…' : 'Xác nhận'}
              </button>
            </div>
          </motion.div>
        </div>
      )}

      {/* ── Modal: Phân công trọng tài ── */}
      {modal === 'referee' && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} className="glass-panel rounded-2xl p-8 w-full max-w-lg border border-cyan-500/20 relative overflow-hidden">
            <div className="absolute top-0 left-8 right-8 h-px bg-gradient-to-r from-transparent via-cyan-400/40 to-transparent pointer-events-none" />
            <div className="absolute -top-10 -right-10 w-40 h-40 rounded-full bg-gradient-to-br from-cyan-500/10 to-transparent blur-[40px] pointer-events-none" />
            <div className="relative flex items-center gap-3 mb-6">
              <div className="w-8 h-8 rounded-lg bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center shrink-0">
                <UserCheck size={15} className="text-cyan-400" />
              </div>
              <h2 className="text-xl font-serif text-white">Phân công trọng tài</h2>
              <div className="flex-1 h-px bg-gradient-to-r from-cyan-400/30 via-glass-border to-transparent" />
            </div>

            <div className="space-y-4">
              <div>
                <label className={LABEL}>Chọn giải đấu *</label>
                <select
                  value={refForm.tournamentId}
                  onChange={e => {
                    setRefForm(p => ({ ...p, tournamentId: e.target.value, raceId: '' }));
                    setReferees([]);
                    setRefViewId('');
                  }}
                  className={INPUT}
                >
                  <option value="">-- Chọn giải đấu --</option>
                  {tournamentsList.map((t: any) => (
                    <option key={t.tournamentId} value={t.tournamentId}>
                      {t.name} #{t.tournamentId}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className={LABEL}>Chọn cuộc đua *</label>
                <select
                  value={refForm.raceId}
                  onChange={e => {
                    setF('raceId', e.target.value);
                    setReferees([]);
                    setRefViewId('');
                  }}
                  disabled={!refForm.tournamentId}
                  className={INPUT}
                >
                  <option value="">
                    {refForm.tournamentId ? '-- Chọn cuộc đua --' : 'Chọn giải đấu trước'}
                  </option>
                  {racesForSelectedRefTournament.map((race: any) => (
                    <option key={race.raceId} value={race.raceId}>
                      {race.name} #{race.raceId} - {race.roundName ?? `Round #${race.roundId}`}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className={LABEL}>Chọn trọng tài *</label>
                <select
                  value={refForm.refereeId}
                  onChange={e => setF('refereeId', e.target.value)}
                  className={INPUT}
                >
                  <option value="">-- Chọn trọng tài --</option>
                  {visibleRefereeOptions.map((ref: any) => (
                    <option key={ref.refereeId} value={ref.refereeId}>
                      {ref.fullName || `Trọng tài #${ref.refereeId}`} {ref.licenseNumber ? `- ${ref.licenseNumber}` : ''}
                    </option>
                  ))}
                </select>
              </div>
              {refError && <div className="text-sm px-4 py-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400">{refError}</div>}
              {refSuccess && <div className="text-sm px-4 py-3 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">{refSuccess}</div>}

              <button onClick={handleAssignReferee} disabled={refLoading} className="w-full py-2.5 rounded-lg bg-cyan-500/20 text-cyan-400 border border-cyan-500/30 hover:bg-cyan-500/30 text-sm font-bold disabled:opacity-60 disabled:cursor-not-allowed transition-colors">
                {refLoading ? 'Đang phân công…' : 'Phân công trọng tài'}
              </button>

              {/* Xem danh sách trọng tài */}
              <div className="pt-2 border-t border-glass-border">
                <div className="flex items-center gap-2 mb-3">
                  <span className="text-xs font-bold text-muted uppercase tracking-wider">Xem trọng tài của cuộc đua</span>
                  <button
                    onClick={() => handleViewReferees(refForm.raceId)}
                    disabled={!refForm.raceId || refViewLoading}
                    className="px-3 py-1 rounded-lg bg-white/5 text-xs text-champagne border border-glass-border hover:bg-white/10 disabled:opacity-40 transition-colors">
                    {refViewLoading ? 'Đang tải…' : 'Tải danh sách'}
                  </button>
                </div>
                {referees.length > 0 && (
                  <div className="space-y-2">
                    {referees.map((r, i) => (
                      <div key={r.id ?? r.refereeId ?? i} className="flex items-center justify-between gap-3 px-4 py-2.5 rounded-lg bg-white/[0.02] border border-glass-border">
                        <div>
                           <div className="text-sm text-white">{r.refereeName ?? r.fullName ?? r.name ?? `Trọng tài #${r.refereeId ?? r.id}`}</div>
                          {r.email && <div className="text-[11px] text-muted">{r.email}</div>}
                        </div>
                        <button
                          onClick={() => handleRemoveReferee(refViewId, r.id ?? r.refereeId)}
                          className="p-1.5 rounded-lg text-red-400 hover:bg-red-500/10 transition-colors">
                          <Trash2 size={14} />
                        </button>
                      </div>
                    ))}
                  </div>
                )}
                {referees.length === 0 && refViewId && !refViewLoading && (
                  <div className="text-center py-4 text-muted text-xs">Chưa có trọng tài nào được phân công</div>
                )}
              </div>
            </div>

            <div className="mt-6">
              <button onClick={closeModal} className="w-full py-2.5 rounded-lg border border-glass-border text-muted hover:text-white hover:bg-white/5 text-sm font-medium transition-colors">Đóng</button>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
}
