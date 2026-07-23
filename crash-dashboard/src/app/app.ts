import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { Chart, registerables } from 'chart.js';
import {
  Summary, DecadeStat, OperatorStat,
  MilCivStat, RegionStat, YearOverYear, DeadliestCrash
} from './services/crash-api.service';

Chart.register(...registerables);

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit, AfterViewInit {

  @ViewChild('decadeCanvas')    decadeCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('milCivCanvas')    milCivCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('operatorsCanvas') operatorsCanvas!: ElementRef<HTMLCanvasElement>;

  summary: Summary | null = null;
  operators: OperatorStat[] = [];
  regions: RegionStat[] = [];
  yoy: YearOverYear[] = [];
  deadliest: DeadliestCrash[] = [];
  milCiv: MilCivStat[] = [];
  loading = true;
  error: string | null = null;

  private decadeData: DecadeStat[] = [];
  private chartsReady = false;
  private dataReady = false;

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    const base = '/api/crashes';
    Promise.all([
      fetch(`${base}/summary`).then(r => r.json()),
      fetch(`${base}/by-decade`).then(r => r.json()),
      fetch(`${base}/top-operators`).then(r => r.json()),
      fetch(`${base}/military-vs-civilian`).then(r => r.json()),
      fetch(`${base}/top-regions`).then(r => r.json()),
      fetch(`${base}/year-over-year`).then(r => r.json()),
      fetch(`${base}/deadliest-per-decade`).then(r => r.json()),
    ]).then(([summary, decades, operators, milCiv, regions, yoy, deadliest]) => {
      this.summary    = summary;
      this.decadeData = decades;
      this.operators  = operators;
      this.milCiv     = milCiv;
      this.regions    = regions;
      this.yoy        = yoy;
      this.deadliest  = deadliest;
      this.loading    = false;
      this.dataReady  = true;
      this.cdr.detectChanges();
      this.tryBuildCharts();
    }).catch(err => {
      this.loading = false;
      this.error = `Could not reach the API. Make sure the .NET backend is running.\n\nDetail: ${err?.message ?? String(err)}`;
      this.cdr.detectChanges();
    });
  }

  ngAfterViewInit() {
    this.chartsReady = true;
    this.tryBuildCharts();
  }

  private tryBuildCharts() {
    if (!this.chartsReady || !this.dataReady) return;
    setTimeout(() => {
      this.buildDecadeChart();
      this.buildMilCivChart();
      this.buildOperatorsChart();
    });
  }

  private buildDecadeChart() {
    new Chart(this.decadeCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: this.decadeData.map(d => `${d.decade}s`),
        datasets: [
          {
            label: 'Crashes',
            data: this.decadeData.map(d => d.crashes),
            backgroundColor: 'rgba(129,140,248,0.75)',
            borderColor: '#818cf8',
            borderWidth: 1,
            borderRadius: 4,
          },
          {
            label: 'Fatalities',
            data: this.decadeData.map(d => d.fatalities),
            backgroundColor: 'rgba(244,114,182,0.55)',
            borderColor: '#f472b6',
            borderWidth: 1,
            borderRadius: 4,
          }
        ]
      },
      options: {
        responsive: true,
        plugins: { legend: { labels: { color: '#94a3b8', boxWidth: 12 } } },
        scales: {
          x: { grid: { color: '#1e1e2e' }, ticks: { color: '#64748b' } },
          y: { grid: { color: '#1e1e2e' }, ticks: { color: '#64748b' } }
        }
      }
    });
  }

  private buildMilCivChart() {
    new Chart(this.milCivCanvas.nativeElement, {
      type: 'doughnut',
      data: {
        labels: this.milCiv.map(d => d.category),
        datasets: [{
          data: this.milCiv.map(d => d.fatalities),
          backgroundColor: ['rgba(251,146,60,0.8)', 'rgba(52,211,153,0.8)'],
          borderColor: ['#fb923c', '#34d399'],
          borderWidth: 2,
          hoverOffset: 6,
        }]
      },
      options: {
        responsive: true,
        cutout: '68%',
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#94a3b8', padding: 16, boxWidth: 12 }
          }
        }
      }
    });
  }

  private buildOperatorsChart() {
    new Chart(this.operatorsCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: this.operators.map(d => d.operator),
        datasets: [{
          label: 'Fatalities',
          data: this.operators.map(d => d.fatalities),
          backgroundColor: this.operators.map((_, i) =>
            `hsla(${230 + i * 8}, 70%, 65%, 0.75)`),
          borderWidth: 0,
          borderRadius: 4,
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        plugins: { legend: { display: false } },
        scales: {
          x: { grid: { color: '#1e1e2e' }, ticks: { color: '#64748b' } },
          y: { grid: { color: 'transparent' }, ticks: { color: '#94a3b8' } }
        }
      }
    });
  }

  fmt(n: number | null | undefined): string {
    if (n == null) return '—';
    return n.toLocaleString();
  }

  pctColor(pct: number | null): string {
    if (pct == null) return 'var(--muted)';
    return pct > 0 ? 'var(--pink)' : 'var(--green)';
  }

  pctArrow(pct: number | null): string {
    if (pct == null) return '';
    return pct > 0 ? '▲' : '▼';
  }
}
