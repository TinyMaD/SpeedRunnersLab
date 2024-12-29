<template>
  <v-container>
    <v-row justify="center">
      <div v-if="!response.data">
        <v-skeleton-loader
          type="table-thead,table-tbody,table-tbody,table-tbody"
          style="width: 940px;"
          class="mx-auto"
        />
      </div>
      <table v-else class="table" :style="{color:$vuetify.theme.dark? 'rgb(198, 212, 223)':'#333'}">
        <tr id="tableHead" :class="$vuetify.theme.dark?'even-dark':'even'">
          <th id="thf" width="10%" />
          <th id="online" width="13%" style="text-align:center;color:#81A636" />
          <th width="33%">{{ $t('rank.personaName') }}</th>
          <th width="27%">{{ $t('rank.tier') }}</th>
          <th v-if="!pc" id="thl" width="17%">{{ $t('rank.score') }}</th>
          <th v-else width="12%">{{ $t('rank.score') }}</th>
          <th v-if="pc" id="thl" width="5%" />
        </tr>
        <tbody id="tbody">
          <tr v-for="(player, index) in response.data" :key="index" :class="$vuetify.theme.dark?(index % 2 === 0 ? 'odd-dark' : 'even-dark'): (index % 2 === 0 ? 'odd' : 'even')">
            <td v-if="index < 3"><img class="rowImg" :src="`img/${index + 1}.png`"></td>
            <td v-else>{{ index+1 }} </td>
            <td class="avator">
              <div class="avatorImg" :style="getAvatorStyle(player)">
                <div v-if="isPlayingSR(player)" class="right">
                  <img class="srIndex" style="height:20px;margin:27px -15px 0 0" src="img/srstate.png">
                </div>
              </div>
            </td>
            <td style="white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">{{ player.personaName }}</td>
            <td>
              <div :class="pc?'levelAll-pc':'levelAll'">
                <div :class="pc?'levelDiv-pc left':'levelDiv left'">
                  <img :class="$vuetify.theme.dark?'level-dark':'level'" src="img/LeagueBadgesMedium.png" :style="getLevelStyle(player.rankLevel)">
                </div>
                <div :class="pc?'levelName-pc left':'levelName left'">{{ getLevelName(player.rankLevel) }}</div>
              </div>
            </td>
            <td>{{ player.rankScore }}
              <div v-if="!pc" style="color:#00C853">{{ scoreIncrease(player) }}</div></td>
            <td v-if="pc" style="text-align:left;color:#00C853">{{ scoreIncrease(player) }}</td>
          </tr>
        </tbody>
        <tr id="tfoot" :class="$vuetify.theme.dark?'even-dark':'even'" :style="{color:$vuetify.theme.dark? 'rgb(189, 187, 185)':'#333'}">
          <td v-if="!pc" id="tfootf" />
          <td v-if="!pc" />
          <td v-if="!pc" id="tfootl" colspan="3">{{ $t('rank.updateInfo') }}</td>
          <td v-if="pc" id="tfootf" />
          <td v-if="pc" />
          <td v-if="pc" />
          <td v-if="pc" id="tfootl" colspan="3">{{ $t('rank.updateInfo') }}</td>
        </tr>
      </table>
    </v-row>
  </v-container>
</template>

<script>
import { getRankList } from "@/api/rank";
export default {
  name: "Rank",

  data() {
    return {
      response: {}
    };
  },

  computed: {
    pc() {
      return window.innerWidth > 1640;
    },
    imgSize() {
      return window.innerWidth < 451 ? 120 : 150;
    },
    leveMarginTop() {
      return window.innerWidth < 451 ? 11 : 6;
    },
    scoreIncrease() {
      return player => {
        const increaseScore = player.rankScore - player.oldRankScore;
        if (increaseScore > 0) {
          return `↑${increaseScore}`;
        }
        return "";
      };
    }
  },

  mounted() {
    getRankList().then(res => {
      this.response = res;
    });
  },

  methods: {
    getAvatorStyle(player) {
      let borderColor = "#4C94B0";
      if (player.state === 0) {
        borderColor = "#616161";
      } else if (player.gameID !== "" && player.gameID != null) {
        borderColor = "#81A636";
      }
      const style = `width: 41px; height: 41px; border: 2px solid ${borderColor}; border-radius: 2px; background-image: url(${player.avatarM}); background-size: 37px; margin: auto;`;
      return style;
    },
    isPlayingSR(player) {
      return player.gameID === "207140";
    },
    getLevelStyle(rankLevel) {
      const height = (4 * this.imgSize) / 3;
      const width = this.imgSize;

      const levelIncrement = width / 3;
      const levelMultiplier = rankLevel % 3;
      const levelOffset = Math.floor(rankLevel / 3);

      const t = (height / 4) * levelOffset;
      const right = levelIncrement * (levelMultiplier + 1);
      const bottom = (height / 4) * (levelOffset + 1);
      const left = levelIncrement * levelMultiplier;

      return `clip: rect(${t}px ${right}px ${bottom}px ${left}px); margin: ${-1 * t + this.leveMarginTop}px 0 0 ${-1 * left}px; width: ${this.imgSize}px;`;
    },
    getLevelName(rankLevel) {
      switch (rankLevel) {
        case 0:
          return this.$t("rank.entry");
        case 1:
          return this.$t("rank.beginner");
        case 2:
          return this.$t("rank.advanced");
        case 3:
          return this.$t("rank.expert");
        case 4:
          return this.$t("rank.bronze");
        case 5:
          return this.$t("rank.silver");
        case 6:
          return this.$t("rank.gold");
        case 7:
          return this.$t("rank.platinum");
        case 8:
          return this.$t("rank.diamond");
        case 9:
          return "KOS";
        default:
          return "";
      }
    }
  },
  metaInfo() {
    return {
      meta: [
        {
          vmid: "keywords",
          name: "keywords",
          content: "SpeedRunners排行榜,SpeedRunners天梯,Rank,league," + this.$baseKeywords
        }
      ]
    };
  }
};
</script>

<style>
.rowImg {
  height: 40px;
  width: 40px;
}

.left {
  float: left;
}

.right {
  float: right;
}

#logdiv {
  height: 105px;
  width: 109px;
}

#floatDiv {
  width: 50%;
}

.table {
  border-collapse: collapse;
  border-spacing: 0;
  margin: 0;
  padding: 0;
  width: 100%;
  max-width: 940px;
  text-align: center;
  vertical-align: middle;
  table-layout: fixed;
}

td {
  align-content: center;
}

th {
  height: 40px;
}

#thf {
  border-radius: 10px 0 0 0;
}

#thl {
  border-radius: 0 10px 0 0;
}

.avator {
  height: 56px;
  padding-top: 4px;
}

.avator img {
  height: 50px;
}

.level-dark {
  position: absolute;
}
.level {
  position: absolute;
  filter: brightness(25%);
}

.levelAll {
  height: 60px;
  width: 108px;
  margin: 0 auto;
}

.levelAll-pc {
  height: 60px;
  width: 135px;
  margin: 0 auto;
}

.levelDiv {
  width: 40px;
  height: 60px;
  position: relative;
  text-align: left;
}

.levelDiv-pc {
  width: 55px;
  height: 60px;
  position: relative;
  text-align: left;
}

.levelName {
  width: 68px;
  height: 41px;
  padding-top: 19px;
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
  text-align: left;
}
.levelName-pc {
  width: 80px;
  height: 41px;
  padding-top: 19px;
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
  text-align: left;
}

#tfoot {
  text-align: right;
  height: 30px;
  font-size: 10px;
}

#tfootf {
  border-radius: 0 0 0 10px;
}

#tfootl {
  border-radius: 0 0 10px 0;
  padding-right: 10px;
}
.even-dark {
  background-color: #262626;
}

.odd-dark {
  background-color: #3E3E3E;
}
.even {
  background-color: #ffffff;
}

.odd {
  background-color: #e0e0e0;
}
</style>