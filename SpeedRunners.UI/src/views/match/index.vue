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
              <p style="padding-top:4rem;color:#e4c269;letter-spacing:.3rem;font-weight: 700">当前总奖金</p>
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
            <div class="title text-h4 pa-2" v-text="'赛程'" />
            <iframe src="https://challonge.com/zh_CN/sxl2/module" width="100%" height="700" frameborder="0" scrolling="auto" allowtransparency="true" />
          </v-sheet>

          <!-- <v-sheet width="100%" class="sheett">
            <div class="title text-h4 pa-2" v-text="'前言'" />
            <div
              v-for="(content,i) in qianyan"
              :key="i"
              class="text-body-1 pa-1 my-1"
              v-text="content"
            />
          </v-sheet> -->

          <v-sheet width="100%" class="sheett">
            <div class="title text-h4 pa-2" v-text="'报名玩家'" />
            <v-simple-table>
              <template v-slot:default>
                <thead>
                  <tr>
                    <th />
                    <th />
                    <th>昵 称</th>
                    <th>天梯分</th>
                    <th>总时长</th>
                    <th>最近两周时长</th>
                    <th>资 质</th>
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
            <div class="title text-h4 pa-2" v-text="'参赛奖励'" />
            <div class="text-body-1 pa-1 my-1" v-text="'前10名选手将获得成就徽章（仅在本站展示）：'" />
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
            <div class="title text-h4 pa-2" v-text="'赞助'" />
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
                    <th>赞助者</th>
                    <th>金 额</th>
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
            <div class="text-body-1 pa-1 my-1" v-text="'赞助方式（扫下方二维码）：'" />
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
            <div class="text-body-1 pa-1 my-1" v-text="'其他赞助方式请联系站长（邮箱）supremelang@qq.com'" />
          </v-sheet>
          <v-sheet
            class="sheett"
            width="100%"
          >
            <div class="title text-h4 pa-2" v-text="'比赛时间'" />
            <div
              class="text-body-1 pa-1 my-1"
              v-text="'小组赛：2023.9.23 ~ 2023.9.24（每日19点开始）'"
            />
            <div
              class="text-body-1 pa-1 my-1"
              v-text="'决 赛：2023.9.30 ~ 2023.10.2（每日19点开始）'"
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
          提 示
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
    prizeList: [],
    qianyan: [
      "由于SR圈环境的特殊性，圈内少有专注竞技性的比赛。公认的高端赛——King of Speed，国内玩家虽有在其名列前茅的水平，但苦于高延迟，而不能愉快的玩耍。",
      "此时，国内首个专注于竞技性的SR比赛——神行令，由此诞生！",
      "公正严谨的赛制、实力强劲的对手、超低的延迟，将助你激发真正的实力，捍卫国服荣耀。没错，这个试炼就是为你而准备，你才是真正的国服前十！",
      "神行令出，群雄逐鹿，鹿死谁手，你我......拭目以待！"
    ],
    sponsorContent: [
      "感谢所有的赞助者，无论金额大小，正因为有大家的支持，本赛事才得以举办",
      "本赛事奖金全部由赞助者赞助，赞助者赞助的资金将100%进入总奖金池",
      "为保证赞助金公开透明，赞助时请备注好你的ID（独特、你自己认识），未备注将按匿名标识记录。站长将在收到赞助后24小时之内维护赞助者名单，赞助者若在名单内未找到自己ID，可到QQ群（319422487）进行反馈",
      "赞助者名单："
    ],
    matchContent: [
      {
        title: "参赛资格",
        content: [
          "中文沟通无障碍、与裁判网络延迟低的玩家均可报名",
          "“资质”排名前12位的报名选手将获得参赛资格",
          "“资质” = “天梯分” + “游戏时长分”",
          "“天梯分” ： 排位赛获取的总分数",
          "“游戏时长分” = “有效游戏时长” X 10 。注：游戏时长均为SpeedRunners的游戏时长，单位为小时，请将你的Steam个人隐私权限设置为公开",
          "“有效游戏时长” = 总游戏时长 - 最近两周游戏时长。注：“有效游戏时长”超过1000小时，按1000小时计算，“最近两周”为报名截止前两周，与报名时间无关",
          "例：小明天梯分为10000，总游戏时长为1200小时，报名截止时最近两周游戏时长300小时，他的资质为10000 + (1200 - 300) X 10 = 19000",
          " ",
          "“资质”排名前4位选手直接锁定胜者组8个名额中的4个",
          "“资质”排名后8位选手进入小组赛"
        ]
      },
      {
        title: "小组赛阶段",
        content: [
          "赛制：循环1v1（即每个选手都会与其余7位选手比赛）",
          "场次：3场",
          "地图：随机官图、工坊图",
          "B/P：不ban地图，直接点官图右下角随机官图和随机工坊图按钮（工坊图、官图交替进行）",
          "采取记章、记场（按章记分后，分数相同的选手会按获胜场数排序）",
          "小组赛前4位选手进入胜者组",
          "小组赛后4位选手进入败者组"
        ]
      },
      {
        title: "决赛阶段",
        content: [
          "赛制：双败制，强强对阵",
          "场次：胜者组半决赛、败者组决赛、总决赛抢8（bo15），其余场抢6（bo11）",
          "地图：待定",
          "B/P：待定",
          "热身赛：胜者组半决赛、败者组决赛、总决赛，赛前进行两场热身赛，热身赛结果不影响比赛成绩，随机官图、工坊图各一张"
        ]
      },
      {
        title: "其他信息",
        content: [
          "请参赛者加神行令比赛QQ群：743740882",
          "努力完善中，请持续保持关注......",
          "※最终解释权归本站所有"
        ]
      }
    ]
  }),
  computed: {
    ...mapGetters([
      "avatar",
      "rankType",
      "participate"
    ]),
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
      var prizeText = ["奖金（随当前总奖金变动）："];
      this.prizeList.forEach(function(item, index, array) {
        prizeText.push(`第 ${index + 1} 名： ${item} 元`);
      });
      var speRuleText = [];
      if (this.speRuleList.length > 0) {
        speRuleText.push("额外奖金：");
        this.speRuleList.forEach(function(item, index, array) {
          speRuleText.push(`${item.speRule}`);
        });
      }
      return prizeText.concat(
        `第 ${this.prizeList.length + 1} 名 ~ 第 12 名：ROLL 50 元`,
        speRuleText,
        "奖金算法：",
        "每上升一个名次，奖金翻倍，差额最大为500元",
        "总奖金增至2900元时，开设第六名奖金即50元，总奖金增至4700元时，开设第七名奖金即50元，以此类推",
        "计算值向下取整，舍去的金额之和加在最低名次奖金上",
        "获得无固定奖金名次的参赛选手中ROLL50元",
        "额外奖金由赞助者自定义发放规则，不参与算法计算");
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