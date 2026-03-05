<template>
  <div class="score-heatmap">
    <div class="heatmap-title">
      <span class="d-flex align-center">
        <v-icon left size="18" class="mr-2" style="opacity:0.5">mdi-fire</v-icon>
        {{ $t('profile.scoreHeatmap') }}
      </span>
      <span class="total-score">
        {{ $t('profile.totalAdded') }}: 
        <span class="score-value">+{{ totalScore }}</span>
      </span>
    </div>
    
    <div class="heatmap-container">
      <!-- 月份标签 -->
      <div class="month-labels">
        <span v-for="(month, index) in monthLabels" :key="index" class="month-label">
          {{ month }}
        </span>
      </div>
      
      <!-- 热度图主体 -->
      <div class="heatmap-body">
        <!-- 星期标签 -->
        <div class="week-labels">
          <span class="week-label"></span>
          <span class="week-label">{{ $t('profile.mon') }}</span>
          <span class="week-label"></span>
          <span class="week-label">{{ $t('profile.wed') }}</span>
          <span class="week-label"></span>
          <span class="week-label">{{ $t('profile.fri') }}</span>
          <span class="week-label"></span>
        </div>
        
        <!-- 格子区域 -->
        <div class="cells-container">
          <div 
            v-for="(week, weekIndex) in weeks" 
            :key="weekIndex" 
            class="week-column"
          >
            <v-tooltip 
              v-for="(day, dayIndex) in week" 
              :key="dayIndex" 
              top
              :disabled="!day.date"
            >
              <template v-slot:activator="{ on, attrs }">
                <div 
                  class="day-cell"
                  :class="getCellClass(day.score)"
                  :style="day.date ? '' : 'visibility: hidden;'"
                  v-bind="attrs"
                  v-on="on"
                />
              </template>
              <span v-if="day.date">
                <strong>{{ day.score > 0 ? '+' : '' }}{{ day.score }}</strong> {{ $t('profile.scoreUnit') }}
                <br>
                {{ formatDate(day.date) }}
              </span>
            </v-tooltip>
          </div>
        </div>
      </div>
      
      <!-- 图例 -->
      <div class="legend">
        <span class="legend-label">{{ $t('profile.less') }}</span>
        <div class="legend-cell level-0" />
        <div class="legend-cell level-1" />
        <div class="legend-cell level-2" />
        <div class="legend-cell level-3" />
        <div class="legend-cell level-4" />
        <span class="legend-label">{{ $t('profile.more') }}</span>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: "ScoreHeatmap",
  
  props: {
    // 每日分数数据，格式: [{ date: '2024-01-01', score: 100 }, ...]
    data: {
      type: Array,
      default: () => []
    }
  },
  
  computed: {
    // 计算总新增分数
    totalScore() {
      return this.data.reduce((sum, item) => sum + Math.max(0, item.score || 0), 0);
    },
    
    // 生成周数据（52周 + 当前周）
    weeks() {
      const weeks = [];
      const today = new Date();
      const oneDay = 24 * 60 * 60 * 1000;
      
      // 创建日期到分数的映射
      const scoreMap = {};
      this.data.forEach(item => {
        if (item.date) {
          scoreMap[item.date] = item.score || 0;
        }
      });
      
      // 计算365天前的日期
      const startDate = new Date(today.getTime() - 364 * oneDay);
      // 调整到周日开始
      const startDayOfWeek = startDate.getDay();
      const adjustedStart = new Date(startDate.getTime() - startDayOfWeek * oneDay);
      
      // 生成每周数据
      let currentDate = new Date(adjustedStart);
      
      while (currentDate <= today) {
        const week = [];
        for (let i = 0; i < 7; i++) {
          const dateStr = this.formatDateKey(currentDate);
          const isInRange = currentDate >= startDate && currentDate <= today;
          
          week.push({
            date: isInRange ? dateStr : null,
            score: isInRange ? (scoreMap[dateStr] || 0) : 0
          });
          
          currentDate = new Date(currentDate.getTime() + oneDay);
        }
        weeks.push(week);
      }
      
      return weeks;
    },
    
    // 月份标签
    monthLabels() {
      const labels = [];
      const today = new Date();
      const months = this.$t('profile.months');
      
      // 获取过去12个月
      for (let i = 11; i >= 0; i--) {
        const date = new Date(today.getFullYear(), today.getMonth() - i, 1);
        labels.push(months[date.getMonth()]);
      }
      
      return labels;
    }
  },
  
  methods: {
    // 获取格子的等级样式
    getCellClass(score) {
      if (score <= 0) return 'level-0';
      if (score < 50) return 'level-1';
      if (score < 500) return 'level-2';
      if (score < 1000) return 'level-3';
      return 'level-4';
    },
    
    // 格式化日期键
    formatDateKey(date) {
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    },
    
    // 格式化显示日期
    formatDate(dateStr) {
      if (!dateStr) return '';
      const date = new Date(dateStr);
      const monthName = this.$t('profile.months')[date.getMonth()];
      return this.$t('profile.heatmapDate', [monthName, date.getDate(), date.getFullYear()]);
    }
  }
};
</script>

<style scoped>
.score-heatmap {
  padding: 16px;
}

.heatmap-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  padding: 12px 16px 0;
  font-size: 13px;
  font-weight: 600;
  letter-spacing: 0.8px;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.7);
}

.total-score {
  font-size: 12px;
  opacity: 0.8;
}

.score-value {
  color: #4caf50;
  font-weight: 600;
}

.heatmap-container {
  overflow-x: auto;
  overflow-y: hidden;
  padding-bottom: 4px;
}

.heatmap-container::-webkit-scrollbar {
  height: 4px;
}

.heatmap-container::-webkit-scrollbar-track {
  background: transparent;
}

.heatmap-container::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.15);
  border-radius: 2px;
}

.heatmap-container::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.25);
}

.month-labels {
  display: flex;
  margin-left: 32px;
  margin-bottom: 4px;
}

.month-label {
  flex: 1;
  font-size: 10px;
  opacity: 0.6;
  min-width: 40px;
}

.heatmap-body {
  display: flex;
}

.week-labels {
  display: flex;
  flex-direction: column;
  margin-right: 4px;
}

.week-label {
  height: 12px;
  font-size: 10px;
  line-height: 12px;
  opacity: 0.6;
  width: 28px;
  text-align: right;
  padding-right: 4px;
}

.cells-container {
  display: flex;
  gap: 3px;
}

.week-column {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.day-cell {
  width: 12px;
  height: 12px;
  border-radius: 2px;
  cursor: pointer;
  transition: transform 0.1s ease;
}

.day-cell:hover {
  transform: scale(1.2);
}

/* 暗色主题颜色 - 类似Steam/游戏风格 */
.level-0 {
  background-color: #1a1a2e;
}

.level-1 {
  background-color: #16213e;
}

.level-2 {
  background-color: #0f4c75;
}

.level-3 {
  background-color: #3282b8;
}

.level-4 {
  background-color: #00d9ff;
  box-shadow: 0 0 6px rgba(0, 217, 255, 0.5);
}

/* 亮色主题适配 */
.theme--light .level-0 {
  background-color: #ebedf0;
}

.theme--light .level-1 {
  background-color: #9be9a8;
}

.theme--light .level-2 {
  background-color: #40c463;
}

.theme--light .level-3 {
  background-color: #30a14e;
}

.theme--light .level-4 {
  background-color: #216e39;
  box-shadow: none;
}

.legend {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  margin-top: 12px;
  gap: 4px;
}

.legend-label {
  font-size: 10px;
  opacity: 0.6;
  margin: 0 4px;
}

.legend-cell {
  width: 12px;
  height: 12px;
  border-radius: 2px;
}
</style>
