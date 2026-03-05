<template>
  <div class="profile-page">
    <!-- 暗色遮罩层（覆盖在AppMain背景之上） -->
    <div class="page-overlay" />

    <!-- 主内容 -->
    <v-container fluid class="profile-container">
      <!-- 加载骨架屏 -->
      <div v-if="loading" class="skeleton-wrapper">
        <v-row>
          <!-- 顶部融合卡片骨架屏 -->
          <v-col cols="12" class="pt-0 pb-1 px-1">
            <v-card flat tile class="glass-card">
              <div class="hero-card">
                <div class="hero-top">
                  <v-skeleton-loader type="avatar" class="mr-4" />
                  <div class="skeleton-info">
                    <v-skeleton-loader type="heading" class="mb-2" />
                    <v-skeleton-loader type="text" />
                  </div>
                </div>
                <v-divider class="my-4" />
                <div class="hero-stats">
                  <v-skeleton-loader type="text" class="mx-4" />
                  <v-skeleton-loader type="text" class="mx-4" />
                  <v-skeleton-loader type="text" class="mx-4" />
                </div>
              </div>
            </v-card>
          </v-col>
          <v-col cols="12" md="6" class="pa-1">
            <v-card flat tile class="glass-card">
              <v-card-title class="glass-card-title">
                <v-skeleton-loader type="text" width="120" />
              </v-card-title>
              <v-card-text class="pa-0">
                <v-skeleton-loader type="list-item, list-item, list-item, list-item" />
              </v-card-text>
            </v-card>
          </v-col>
          <v-col cols="12" md="6" class="pa-1">
            <v-card flat tile class="glass-card">
              <v-card-title class="glass-card-title">
                <v-skeleton-loader type="text" width="100" />
                <v-spacer />
                <v-skeleton-loader type="text" width="40" />
              </v-card-title>
              <v-card-text>
                <v-skeleton-loader type="text@5" />
              </v-card-text>
            </v-card>
          </v-col>
          <!-- 天梯热力图骨架屏 -->
          <v-col cols="12" class="pa-1">
            <v-card flat tile class="glass-card">
              <v-card-text class="pa-3">
                <v-skeleton-loader type="image" height="120" />
              </v-card-text>
            </v-card>
          </v-col>
        </v-row>
      </div>

      <!-- 主内容区 -->
      <template v-else-if="profileData">
        <v-row>
          <!-- 顶部融合卡片：玩家信息 + 核心数据 -->
          <v-col cols="12" class="pt-0 pb-1 px-1">
            <v-card flat tile class="glass-card">
              <div class="hero-card">
                <!-- 上半部：头像 + 玩家信息 + Steam按钮 -->
                <div class="hero-top">
                  <div class="hero-avatar-wrapper" :class="getOnlineClass">
                    <v-avatar size="72" class="profile-avatar">
                      <v-img :src="profileData.avatarL || profileData.avatarM" />
                    </v-avatar>
                  </div>
                  <div class="hero-info">
                    <h2 class="player-name text-h5 font-weight-bold">
                      {{ profileData.personaName }}
                    </h2>
                    <div class="status-chips mt-1">
                      <v-chip
                        small
                        :color="getStatusChipColor"
                        :text-color="getStatusChipTextColor"
                        class="mr-1"
                      >
                        <span :class="getOnlineClass" />
                        {{ getStatusText }}
                      </v-chip>
                      <div class="level-badge mr-2">
                        <div class="level-icon-wrapper">
                          <img
                            src="/img/LeagueBadgesMedium.png"
                            :style="getLevelIconStyle"
                            class="level-icon"
                          >
                        </div>
                        <span class="level-text">{{ getLevelName }}</span>
                      </div>
                    </div>
                  </div>
                  <v-spacer />
                  <v-btn
                    outlined
                    small
                    class="steam-btn"
                    :href="`https://steamcommunity.com/profiles/${profileData.platformID}`"
                    target="_blank"
                  >
                    <v-icon left small>mdi-steam</v-icon>
                    STEAM
                  </v-btn>
                </div>
                <!-- 下半部：三列核心数据 -->
                <div class="hero-stats">
                  <div class="hero-stat-item">
                    <div class="text-h4 font-weight-bold stat-number">
                      {{ profileData.past2WeeksPlaytime || 0 }}
                    </div>
                    <div class="text-caption text--secondary">
                      {{ $t('rank.past2weeks') }}
                    </div>
                  </div>
                  <v-divider vertical class="stat-divider" /><div class="hero-stat-item">
                    <div class="text-h4 font-weight-bold stat-number">
                      {{ formatNumber(profileData.rankScore) }}
                    </div>
                    <div class="text-caption text--secondary">
                      {{ $t('rank.score') }}
                    </div>
                  </div>
                  <v-divider vertical class="stat-divider" />
                  <div class="hero-stat-item">
                    <div class="text-h4 font-weight-bold stat-number highlight">
                      +{{ formatNumber(profileData.past2WeeksScore || 0) }}
                    </div>
                    <div class="text-caption text--secondary">
                      {{ $t('profile.recentScore') }}
                    </div>
                  </div>
                </div>
              </div>
            </v-card>
          </v-col>

          <!-- 左侧边栏 - 统计 + 成就 -->
          <v-col cols="12" md="6" class="pa-1" order="1" order-md="1">

            <!-- 游戏统计卡片 -->
            <v-card flat tile class="glass-card">
              <v-card-title class="glass-card-title">
                <v-icon left size="18">mdi-chart-bar</v-icon>
                {{ $t('profile.gameStats') }}
              </v-card-title>
              <v-card-text class="pa-0">
                <v-simple-table dense class="stats-table">
                  <template v-slot:default>
                    <tbody>
                      <tr v-for="stat in gameStats" :key="stat.name">
                        <td class="stat-name text-caption">{{ stat.name }}</td>
                        <td class="stat-val text-body-2 font-weight-medium">{{ stat.value }}</td>
                      </tr>
                    </tbody>
                  </template>
                </v-simple-table>
              </v-card-text>
            </v-card>
          </v-col>

          <!-- 成就卡片 -->
          <v-col cols="12" md="6" class="pa-1" order="2" order-md="2">
            <v-card flat tile class="glass-card">
              <v-card-title class="glass-card-title">
                <v-icon left size="18">mdi-medal</v-icon>
                {{ $t('profile.achievements') }}
                <v-spacer />
                <span class="text-caption text--secondary">
                  {{ unlockedCount }}/{{ achievements.length }}
                </span>
              </v-card-title>
              <v-card-text>
                <div v-if="achievements.length === 0" class="no-data text-caption">
                  {{ $t('profile.noAchievements') }}
                </div>
                <div v-else class="achievements-grid">
                  <v-tooltip
                    v-for="achievement in achievements"
                    :key="achievement.id"
                    top
                    color="rgba(20,20,30,0.95)"
                  >
                    <template v-slot:activator="{ on, attrs }">
                      <div
                        class="achievement-item"
                        :class="{ 'unlocked': achievement.unlocked }"
                        v-bind="attrs"
                        v-on="on"
                      >
                        <v-icon
                          :color="achievement.unlocked ? 'amber' : 'grey darken-1'"
                          size="28"
                        >
                          {{ achievement.icon || 'mdi-trophy-variant' }}
                        </v-icon>
                      </div>
                    </template>
                    <div class="achievement-tooltip">
                      <div class="font-weight-bold amber--text">{{ achievement.name }}</div>
                      <div class="text-caption mt-1">{{ achievement.description }}</div>
                      <div v-if="achievement.unlocked" class="unlock-time text-caption mt-1">
                        {{ formatUnlockTime(achievement.unlockedAt) }}
                      </div>
                    </div>
                  </v-tooltip>
                </div>
              </v-card-text>
            </v-card>
          </v-col>

          <!-- 天梯热力图（独占一行） -->
          <v-col cols="12" class="pa-1" order="3">
            <v-card flat tile class="glass-card">
              <v-card-text class="pa-0">
                <ScoreHeatmap :data="dailyScoreData" />
              </v-card-text>
            </v-card>
          </v-col>
        </v-row>
      </template>

      <!-- 未找到用户 -->
      <v-card v-else flat tile class="glass-card not-found-card">
        <v-card-text class="text-center py-12">
          <v-icon size="64" color="grey">mdi-account-question</v-icon>
          <h3 class="mt-4">{{ $t('profile.notFound') }}</h3>
          <p class="mt-2 text--secondary">{{ $t('profile.notFoundDesc') }}</p>
          <v-btn color="primary" to="/" class="mt-4">
            {{ $t('404.back') }}
          </v-btn>
        </v-card-text>
      </v-card>
    </v-container>
  </div>
</template>

<script>
import ScoreHeatmap from "@/components/ScoreHeatmap";
import { getProfileData, getDailyScoreHistory, getAchievements } from "@/api/profile";

export default {
  name: "Profile",

  components: {
    ScoreHeatmap
  },

  data() {
    return {
      loading: true,
      profileData: null,
      dailyScoreData: [],
      achievements: []
    };
  },

  computed: {
    steamId() {
      return this.$route.params.steamId || this.$route.query.id;
    },

    isPlayingSR() {
      return this.profileData && this.profileData.gameID === "207140";
    },

    getOnlineClass() {
      if (!this.profileData) return "offline";
      if (this.profileData.state === 0) return "offline";
      if (this.isPlayingSR) return "playing";
      return "online";
    },

    getStatusText() {
      if (!this.profileData) return this.$t("profile.offline");
      if (this.profileData.state === 0) return this.$t("profile.offline");
      if (this.isPlayingSR) return this.$t("profile.playingSR");
      return this.$t("profile.online");
    },

    getStatusChipColor() {
      if (!this.profileData || this.profileData.state === 0) return "grey darken-2";
      if (this.isPlayingSR) return "light-green darken-1";
      return "green darken-1";
    },

    getStatusChipTextColor() {
      return "white";
    },

    getLevelChipColor() {
      const level = (this.profileData && this.profileData.rankLevel) || 0;
      const colors = ["grey", "grey darken-1", "blue-grey", "blue-grey darken-1", "brown", "grey lighten-1", "amber darken-2", "blue-grey lighten-1", "cyan darken-1", "deep-purple"];
      return colors[level] || "grey";
    },

    getLevelName() {
      const level = (this.profileData && this.profileData.rankLevel) || 0;
      const names = ["entry", "beginner", "advanced", "expert", "bronze", "silver", "gold", "platinum", "diamond"];
      if (level === 9) return "KOS";
      return this.$t("rank." + names[level]);
    },

    getLevelIconStyle() {
      const level = (this.profileData && this.profileData.rankLevel) || 0;
      const size = 80;
      const height = (4 * size) / 3;
      const width = size;

      const levelIncrement = width / 3;
      const levelMultiplier = level % 3;
      const levelOffset = Math.floor(level / 3);

      const top = (height / 4) * levelOffset;
      const left = levelIncrement * levelMultiplier;

      return "top: " + (-1 * top) + "px; left: " + (-1 * left) + "px; width: " + size + "px; height: " + height + "px;";
    },

    gameStats() {
      if (!this.profileData || !this.profileData.stats) return [];
      return this.profileData.stats.map(function(s) {
        return { name: s.name, value: s.value };
      });
    },

    unlockedCount() {
      return this.achievements.filter(function(a) { return a.unlocked }).length;
    },

    rankProgress() {
      // 简单计算进度（可根据实际段位系统调整）
      var score = (this.profileData && this.profileData.rankScore) || 0;
      var level = (this.profileData && this.profileData.rankLevel) || 0;
      // 每个段位大约需要的分数（示例）
      var levelScores = [0, 500, 1000, 1500, 2000, 2500, 3000, 3500, 4000, 5000];
      var current = levelScores[level] || 0;
      var next = levelScores[level + 1] || 5000;
      if (score >= next) return 100;
      return ((score - current) / (next - current)) * 100;
    }
  },

  watch: {
    steamId: {
      immediate: true,
      handler: function(newVal) {
        if (newVal) {
          this.fetchProfileData();
        }
      }
    }
  },

  methods: {
    formatNumber: function(num) {
      if (num == null) return "0";
      return num.toLocaleString();
    },

    fetchProfileData: function() {
      var self = this;
      self.loading = true;

      Promise.all([
        getProfileData(self.steamId).catch(function() { return { data: null } }),
        getDailyScoreHistory(self.steamId).catch(function() { return { data: [] } }),
        getAchievements(self.steamId).catch(function() { return { data: [] } })
      ]).then(function(results) {
        self.profileData = results[0].data;
        self.dailyScoreData = results[1].data || [];
        self.achievements = results[2].data || [];
      }).catch(function(error) {
        console.error("Failed to load profile:", error);
        self.profileData = null;
      }).finally(function() {
        self.loading = false;
      });
    },

    formatUnlockTime: function(time) {
      if (!time) return "";
      var date = new Date(time);
      if (this.$i18n.locale === "zh") {
        return date.getFullYear() + "年" + (date.getMonth() + 1) + "月" + date.getDate() + "日解锁";
      }
      var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
      return "Unlocked " + months[date.getMonth()] + " " + date.getDate() + ", " + date.getFullYear();
    }
  },

  metaInfo: function() {
    var name = (this.profileData && this.profileData.personaName) || "";
    return {
      title: name ? name + " - SpeedRunners Profile" : "Player Profile",
      meta: [
        {
          vmid: "keywords",
          name: "keywords",
          content: "SpeedRunners玩家," + name + ",个人主页,游戏数据"
        }
      ]
    };
  }
};
</script>

<style scoped>
.profile-page {
  position: relative;
  min-height: calc(100vh - 50px);
}

/* 暗色遮罩层 */
.page-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(180deg, rgba(8, 8, 18, 0.35) 0%, rgba(8, 8, 18, 0.6) 40%, rgba(8, 8, 18, 0.82) 100%);
  pointer-events: none;
  z-index: 0;
}

/* 容器 */
.profile-container {
  position: relative;
  z-index: 1;
  /* padding: 8px 16px; */
  max-width: 880px;
  margin: 0 auto;
}

/* ======================= */
/* 毛玻璃卡片通用样式 */
/* ======================= */
.glass-card {
  background: rgba(18, 18, 28, 0.88) !important;
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.08) !important;
  border-radius: 6px !important;
  transition: border-color 0.3s ease, box-shadow 0.3s ease;
}

.glass-card:hover {
  border-color: rgba(255, 255, 255, 0.14) !important;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.3);
}

.glass-card-title {
  font-size: 13px;
  font-weight: 600;
  padding: 14px 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  letter-spacing: 0.8px;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.7);
}

.glass-card-title .v-icon {
  opacity: 0.5;
}

/* ======================= */
/* 顶部融合卡片 */
/* ======================= */
.hero-card {
  padding: 20px 24px;
}

.hero-top {
  display: flex;
  align-items: center;
  gap: 16px;
}

.hero-avatar-wrapper {
  position: relative;
  padding: 3px;
  border-radius: 10px;
  background: rgba(0, 0, 0, 0.4);
  flex-shrink: 0;
  transition: all 0.3s ease;
}

.hero-avatar-wrapper.online {
  box-shadow: 0 0 16px rgba(76, 175, 80, 0.3);
  border: 2px solid rgba(76, 175, 80, 0.6);
}

.hero-avatar-wrapper.playing {
  box-shadow: 0 0 20px rgba(139, 195, 74, 0.4);
  border: 2px solid rgba(139, 195, 74, 0.7);
  animation: glow-pulse 2.5s infinite;
}

.hero-avatar-wrapper.offline {
  border: 2px solid rgba(128, 128, 128, 0.25);
}

.hero-info {
  flex: 1;
  min-width: 0;
}

.hero-stats {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
  gap: 0;
}

.hero-stat-item {
  flex: 1;
  text-align: center;
  padding: 4px 16px;
}

/* ======================= */
/* 左侧个人信息卡片 (已移到hero) */
/* ======================= */

@keyframes glow-pulse {
  0%, 100% { box-shadow: 0 0 20px rgba(139, 195, 74, 0.4); }
  50% { box-shadow: 0 0 35px rgba(139, 195, 74, 0.6); }
}

.profile-avatar {
  border-radius: 6px;
}

.player-name {
  text-align: left;
  letter-spacing: 0.5px;
}

/* 状态标签 */
.status-chips {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-start;
  gap: 6px;
}

/* 段位徽章样式 - 参考 rank 页面实现 */
.level-badge {
  display: inline-flex;
  align-items: center;
  height: 24px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 11px;
  padding: 0 8px 0 4px;
}

.level-icon-wrapper {
  width: 24px;
  height: 24px;
  position: relative;
  overflow: hidden;
  margin-right: 4px;
}

.level-icon {
  position: absolute;
}

.level-text {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  white-space: nowrap;
}

.steam-btn {
  border-color: rgba(255, 255, 255, 0.2) !important;
  color: rgba(255, 255, 255, 0.8) !important;
  text-transform: uppercase;
  letter-spacing: 1px;
  font-size: 11px;
}

.steam-btn:hover {
  background: rgba(255, 255, 255, 0.1) !important;
  border-color: rgba(255, 255, 255, 0.3) !important;
}

/* ======================= */
/* 游戏统计表格 */
/* ======================= */
.stats-table {
  background: transparent !important;
}

.stats-table tbody tr {
  transition: background 0.2s ease;
}

.stats-table tbody tr:hover {
  background: rgba(255, 255, 255, 0.03) !important;
}

.stats-table .stat-name {
  padding: 10px 16px !important;
  color: rgba(255, 255, 255, 0.45);
  font-size: 12px;
}

.stats-table .stat-val {
  padding: 10px 16px !important;
  text-align: right;
  color: rgba(100, 181, 246, 0.85);
  font-size: 13px;
}

/* ======================= */
/* 成就网格 */
/* ======================= */
.achievements-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 8px;
}

.achievement-item {
  aspect-ratio: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.05);
  cursor: pointer;
  transition: all 0.25s ease;
}

.achievement-item:hover {
  transform: translateY(-2px);
  background: rgba(255, 255, 255, 0.08);
  border-color: rgba(255, 255, 255, 0.15);
}

.achievement-item.unlocked {
  background: rgba(255, 193, 7, 0.1);
  border-color: rgba(255, 193, 7, 0.25);
}

.achievement-item.unlocked:hover {
  box-shadow: 0 4px 20px rgba(255, 193, 7, 0.2);
}

.achievement-tooltip {
  text-align: center;
  max-width: 200px;
  padding: 4px;
}

.unlock-time {
  color: #81c784;
}

/* ======================= */
/* 核心数据 */
/* ======================= */
.stat-number {
  line-height: 1;
  letter-spacing: -0.5px;
  color: rgba(255, 255, 255, 0.95);
}

.stat-number.highlight {
  color: #66bb6a;
  text-shadow: 0 0 20px rgba(102, 187, 106, 0.35);
}

.stat-divider {
  height: 40px;
  opacity: 0.1;
}

/* ======================= */
/* 无数据 & 未找到 */
/* ======================= */
.no-data {
  text-align: center;
  padding: 24px;
  color: rgba(255, 255, 255, 0.5);
}

.not-found-card {
  max-width: 500px;
  margin: 60px auto;
}

/* ======================= */
/* 骨架屏适配毛玻璃 */
/* ======================= */
.skeleton-wrapper {
  width: 100%;
  max-width: 100%;
}

.skeleton-wrapper .row {
  width: 100%;
  margin: 0;
}

.skeleton-wrapper .v-card {
  width: 100%;
}

.skeleton-wrapper .v-skeleton-loader {
  background: transparent !important;
}

.skeleton-wrapper .hero-card .v-skeleton-loader {
  background: transparent !important;
  border: none;
  border-radius: 0;
}

.skeleton-wrapper .hero-stats {
  display: flex;
  justify-content: center;
}

.skeleton-wrapper .skeleton-info {
  flex: 1;
  min-width: 0;
}

.skeleton-wrapper .skeleton-info .v-skeleton-loader {
  max-width: 200px;
}

/* ======================= */
/* 响应式 */
/* ======================= */
@media (max-width: 960px) {
  .profile-container {
    padding: 16px;
  }

  .achievements-grid {
    grid-template-columns: repeat(5, 1fr);
  }

  .stat-number {
    font-size: 1.5rem !important;
  }
}

@media (max-width: 600px) {
  .stat-divider {
    display: none;
  }
}
</style>