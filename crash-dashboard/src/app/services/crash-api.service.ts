import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Summary {
  totalCrashes: number;
  totalAboard: number;
  totalFatalities: number;
  fatalityRatePct: number;
}

export interface DecadeStat {
  decade: number;
  crashes: number;
  fatalities: number;
}

export interface OperatorStat {
  operator: string;
  crashes: number;
  fatalities: number;
}

export interface MilCivStat {
  category: string;
  crashes: number;
  fatalities: number;
  avgFatalitiesPerCrash: number;
}

export interface RegionStat {
  region: string;
  crashes: number;
  fatalities: number;
}

export interface YearOverYear {
  year: number;
  crashes: number;
  previousYearCrashes: number | null;
  pctChange: number | null;
}

export interface DeadliestCrash {
  decade: number;
  date: string;
  location: string;
  operator: string;
  fatalities: number;
}

const API = '/api/crashes';

@Injectable({ providedIn: 'root' })
export class CrashApiService {
  private http = inject(HttpClient);

  getSummary(): Observable<Summary> {
    return this.http.get<Summary>(`${API}/summary`);
  }

  getByDecade(): Observable<DecadeStat[]> {
    return this.http.get<DecadeStat[]>(`${API}/by-decade`);
  }

  getTopOperators(top = 10): Observable<OperatorStat[]> {
    return this.http.get<OperatorStat[]>(`${API}/top-operators?top=${top}`);
  }

  getMilitaryVsCivilian(): Observable<MilCivStat[]> {
    return this.http.get<MilCivStat[]>(`${API}/military-vs-civilian`);
  }

  getTopRegions(top = 10): Observable<RegionStat[]> {
    return this.http.get<RegionStat[]>(`${API}/top-regions?top=${top}`);
  }

  getYearOverYear(lastYears = 10): Observable<YearOverYear[]> {
    return this.http.get<YearOverYear[]>(`${API}/year-over-year?lastYears=${lastYears}`);
  }

  getDeadliestPerDecade(): Observable<DeadliestCrash[]> {
    return this.http.get<DeadliestCrash[]>(`${API}/deadliest-per-decade`);
  }
}
