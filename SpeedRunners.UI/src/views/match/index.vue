<template>
  <v-container fluid>
    <v-row justify="center">
      <v-card width="1500px" min-height="800px" dark tile>
        <v-row style="padding:20px">
          <v-img
            src="img/sxlbg.jpg"
            gradient="to bottom, rgba(30,30,30,0), rgba(30,30,30,1)"
          >
            <v-row class="d-flex justify-center">
              <div><v-img
                style="margin-top:5px"
                width="450"
                src="img/sxltext.png"
              /></div>
            </v-row>

            <v-row class="d-flex justify-center">
              <p style="padding-top:4rem;color:#e4c269;letter-spacing:.3rem;font-weight: 700">{{ $t('match.prizePool') }}</p>
            </v-row>

            <v-col cols="12" class="d-flex justify-center" style="font-size: 6rem!important;line-height: 6rem;font-weight: 700!important;font-family: Roboto,sans-serif!important;letter-spacing:.5rem;margin-top:-2rem">
              <p style="color:#e4c269;padding-top:5px;">¥</p>
              <Odometer :value="prizePool" color="#e4c269" :duration="1500" />
            </v-col>

            <!-- <v-row class="d-flex justify-center">
              <v-btn
                class="mt-10 baom-btn text-h-4"
                color="rgb(39,31,10)"
                @click.stop="showDialog"
              >
                {{ btnText }}
              </v-btn>
            </v-row> -->
          </v-img>

          <v-sheet width="100%" class="sheett">
            <div class="title text-h4 pa-2" v-text="$t('match.schedule')" />
            <iframe :src="scheduleSrc" width="100%" height="700" frameborder="0" scrolling="auto" allowtransparency="true" />
          </v-sheet>

          <v-sheet width="100%" class="sheett">
            <div class="title text-h4 pa-2" v-text="$t('match.participant')" />
            <v-simple-table>
              <template v-slot:default>
                <thead>
                  <tr>
                    <th />
                    <th />
                    <th>{{ $t('rank.personaName') }}</th>
                    <th>{{ $t('rank.score') }}</th>
                    <th>{{ $t('rank.playTime') }}</th>
                    <th>{{ $t('rank.past2weeks') }}</th>
                    <th>{{ $t('match.points') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(item,index) in participateList" :key="index">
                    <td>{{ index + 1 }}</td>
                    <td><v-avatar size="35"><img :src="item.avatarM"></v-avatar></td>
                    <td>{{ item.personaName }}</td>
                    <td>{{ item.rankScore }}</td>
                    <td>{{ item.playTime }}</td>
                    <td>{{ item.weekPlayTime }}</td>
                    <td>{{ item.sxlScore }}</td>
                  </tr>
                </tbody>
              </template>
            </v-simple-table>
          </v-sheet>

          <v-sheet width="100%" class="sheett">
            <div class="title text-h4 pa-2" v-text="$t('match.reward')" />
            <div class="text-body-1 pa-1 my-1" v-text="$t('match.badgeContent')" />
            <v-tooltip v-for="i in 10" :key="i-100" top>
              <template v-slot:activator="{ on, attrs }">
                <svg-icon
                  v-bind="attrs"
                  class="text-h4"
                  :icon-class="`sxl-${i}`"
                  v-on="on"
                />
              </template>
              <span>{{ badgeText(i) }}</span>
            </v-tooltip>
            <div
              v-for="(prize,i) in prizeText"
              :key="i"
              class="text-body-1 pa-1 my-1"
              v-text="prize"
            />
          </v-sheet>

          <v-sheet width="100%" class="sheett">
            <div class="title text-h4 pa-2" v-text="$t('match.support')" />
            <div
              v-for="(content,i) in sponsorContent"
              :key="i"
              class="text-body-1 pa-1 my-1"
              v-text="content"
            />
            <v-simple-table style="width:500px">
              <template v-slot:default>
                <thead>
                  <tr>
                    <th>{{ $t('match.sponsor') }}</th>
                    <th>{{ $t('match.amount') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(item,index) in sponsorList" :key="index">
                    <td>{{ item.name }}</td>
                    <td>{{ `${item.amount} 元` }}</td>
                  </tr>
                </tbody>
              </template>
            </v-simple-table>
            <div class="text-body-1 pa-1 my-1" v-text=" $t('match.sponsorship') " />
            <v-tooltip right>
              <template v-slot:activator="{ on, attrs }">
                <svg-icon
                  style="background-color:rgb(6,180,253);border-radius:50%"
                  v-bind="attrs"
                  class="text-h5"
                  icon-class="zfb"
                  v-on="on"
                />
              </template>
              <v-img max-width="210" src="img/zfb.png" />
            </v-tooltip>
            <v-tooltip right>
              <template v-slot:activator="{ on, attrs }">
                <svg-icon
                  style="color:white;background-color:rgb(42,174,103);border-radius:50%"
                  v-bind="attrs"
                  class="text-h5"
                  icon-class="wx"
                  v-on="on"
                />
              </template>
              <v-img max-width="210" src="img/wx.png" />
            </v-tooltip>
            <div class="text-body-1 pa-1 my-1" v-text="$t('match.contactMe',['supremelang@qq.com'])" />
          </v-sheet>
          <v-sheet
            class="sheett"
            width="100%"
          >
            <div class="title text-h4 pa-2" v-text="$t('match.time')" />
            <div
              v-for="(item,index) in timeContent"
              :key="index"
              class="text-body-1 pa-1 my-1"
              v-text="item"
            />
            <v-img max-width="700px" src="img/rl102.png" />
          </v-sheet>

          <v-sheet
            v-for="(item,index) in matchContent"
            :key="index"
            class="sheett"
            width="100%"
          >
            <div class="title text-h4 pa-2" v-text="item.title" />
            <div
              v-for="(content,i) in item.content"
              :key="i"
              class="text-body-1 pa-1 my-1"
              v-text="content"
            />
          </v-sheet>
        </v-row>
      </v-card>
    </v-row>
    <v-dialog
      v-model="dialog"
      :persistent="participate === 0"
      width="500"
    >
      <v-card dark>
        <v-card-title class="text-h5">
          {{ $t('common.notice') }}
        </v-card-title>
        <v-card-text>
          {{ participateText }}
        </v-card-text>
        <v-divider />
        <v-card-actions>
          <v-btn
            color="primary"
            :text="participate !== 0"
            :loading="loading"
            :disabled="loading"
            @click="updateParticipate"
          >
            {{ saveBtnText }}
          </v-btn>
          <v-spacer />
          <v-btn
            color="primary"
            :text="participate === 0"
            :disabled="loading"
            @click="dialog = false"
          >
            {{ cancelBtnText }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>
<script>
import Odometer from "@/components/Odometer";
import { getSponsor, updateParticipate, getParticipateList } from "@/api/rank";
import { mapGetters } from "vuex";
import { sectionToChinese } from "@/utils";
export default {
  name: "Match",
  components: {
    Odometer
  },
  data: () => ({
    loading: false,
    dialog: false,
    prizePool: 0,
    sponsorList: [],
    participateList: [],
    speRuleList: [],
    prizeList: []
  }),
  computed: {
    ...mapGetters([
      "avatar",
      "rankType",
      "participate"
    ]),
    matchContent() {
      return this.$t("match.matchContent");
    },
    sponsorContent() { return this.$t("match.sponsorContent") },
    timeContent() {
      return [this.$t("match.groupStage") + "：2023.9.23 ~ 2023.9.24（每日19点开始）", this.$t("match.finalStage") + "：2023.9.30 ~ 2023.10.2（每日19点开始）"];
    },
    scheduleSrc() {
      return `https://challonge.com/${(this.$i18n.locale === "zh" ? "zh_CN" : "en")}/sxl2/module`;
    },
    btnText() {
      return this.participate === 0 ? "我要报名" : "您已报名";
    },
    participateText() {
      return this.participate === 0 ? "为便于赛程统计，比赛期间，您在本站的Steam昵称将停止更新。" : "少侠留步！";
    },
    saveBtnText() {
      return this.participate === 0 ? "报 名" : "狠心取消报名";
    },
    cancelBtnText() {
      return this.participate === 0 ? "取 消" : "陪你们玩玩";
    },
    prizeText() {
      var that = this;
      var prizeText = [this.$t("match.prize") + "："];
      this.prizeList.forEach(function(item, index, array) {
        prizeText.push(that.$t("match.standings", [index + 1]) + `： ${item} 元`);
      });
      var speRuleText = [];
      if (this.speRuleList.length > 0) {
        speRuleText.push(this.$t("match.additional") + "：");
        this.speRuleList.forEach(function(item, index, array) {
          speRuleText.push(`${item.speRule}`);
        });
      }
      return prizeText.concat(
        `${that.$t("match.standings", [this.prizeList.length + 1])} ~ ${that.$t("match.standings", [12])}：ROLL 50 元`,
        speRuleText,
        this.$t("match.algorithm"));
    },
    badgeText() {
      return function(num) {
        const numText = sectionToChinese(num);
        return `2023国服第${numText === "一十" ? "十" : numText}`;
      };
    }
  },
  mounted() {
    getSponsor().then(response => {
      this.sponsorList = response.data;
      var normalRuleList = this.sponsorList.filter(item => item.speRule == null);
      this.speRuleList = this.sponsorList.filter(item => item.speRule != null);

      this.prizePool = this.sponsorList.reduce((a, b) => a + b.amount, 0);
      var normalPrizePool = normalRuleList.reduce((a, b) => a + b.amount, 0);

      this.getPrizeBase(50, normalPrizePool - 50, this.prizeList);
      var total = normalPrizePool - 50 - this.prizeList.reduce((a, b) => a + b, 0);
      this.calculate(this.prizeList, total);
      this.prizeList.reverse();
      for (let i = 0; i < this.prizeList.length; i++) {
        this.prizeList[i] = parseInt(this.prizeList[i]);
      }
      var prizeAll = this.prizeList.reduce((a, b) => a + b, 0);
      var balance = normalPrizePool - 50 - prizeAll;
      this.prizeList[this.prizeList.length - 1] += balance;
    });
    this.getParticipators();
  },
  methods: {
    showDialog() {
      if (this.avatar === "") {
        this.$toast.info("请先登录后，再报名");
        return;
      }
      if (this.rankType === 0) {
        this.$toast.error("您的账号还未拥有SpeedRunners游戏");
        return;
      }
      this.dialog = true;
    },
    updateParticipate() {
      this.loading = true;
      var that = this;
      const participate = this.participate === 0;
      updateParticipate(participate).then(res => {
        if (res.data) {
          that.$store.dispatch("user/getInfo");
          that.dialog = false;
          that.$toast.success((participate ? "" : "取消") + "报名成功");
          that.getParticipators();
        } else {
          that.$toast.error((participate ? "" : "取消") + "报名失败，请稍后再试");
        }
        that.loading = false;
      }).catch(() => {
        that.loading = false;
      });
    },
    getParticipators() {
      getParticipateList().then(response => {
        this.participateList = response.data;
      });
    },
    getPrizeBase(prize, total, prizeList) {
      if (total >= prize) {
        prizeList.push(prize);
        total -= prize;
        prize = prize > 500 ? prize + 500 : prize * 2;
        this.getPrizeBase(prize, total, prizeList);
      }
    },
    calculate(prizeList, total) {
      const partList = [];
      const index = prizeList.findIndex(x => x > 500);
      for (let i = 0; i < prizeList.length; i++) {
        let part = 1;
        if (i <= index) {
          part = Math.pow(2, i);
        } else {
          part = partList[index];
        }
        partList.push(part);
      }
      const partAll = partList.reduce((a, b) => a + b, 0);
      const avg = total / partAll;
      if (avg * partList[partList.length - 1] <= 1) return;
      for (let i = 0; i < prizeList.length; i++) {
        let money = avg * partList[i];
        money = (i > 0 && prizeList[i] + money - prizeList[i - 1] > 500) ? prizeList[i - 1] + 500 - prizeList[i] : money;
        prizeList[i] += money;
        total -= money;
      }
      this.calculate(prizeList, total);
    }
  }
};
</script>
<style lang="scss" scoped>
  .title {
    color: #e4c269;
    margin: 0 !important;
    border-bottom: 2px solid #e4c269;
    letter-spacing: 0 !important;
  }
  .baom-btn {
    color: #e4c269;
    border: 1px solid #e4c269!important;
    font-size: 25px!important;
    font-weight: 700!important;
    letter-spacing:3px;
    width: 180px !important;
    height: 60px !important;
  }
  .sheett{
    padding-bottom: 1rem;
  }
</style>