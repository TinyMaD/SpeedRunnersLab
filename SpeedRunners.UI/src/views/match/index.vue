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
                width="500"
                src="img/sxltext.png"
              /></div>
            </v-row>
            <v-row class="d-flex justify-center">
              <p style="padding-top:5rem;color:#e4c269;letter-spacing:.3rem;font-weight: 700">当前总奖金</p>
            </v-row>
            <v-col cols="12" class="d-flex justify-center" style="font-size: 6rem!important;line-height: 6rem;font-weight: 700!important;font-family: Roboto,sans-serif!important;letter-spacing:.5rem;margin-top:-2rem">
              <p style="color:#e4c269;padding-top:5px;">¥</p>
              <Odometer :value="prizePool" color="#e4c269" />
            </v-col>
          </v-img>
        </v-row>
      </v-card>
    </v-row>
  </v-container>
</template>
<script>
import Odometer from "@/components/Odometer";
export default {
  name: "Match",
  components: {
    Odometer
  },
  data: () => ({
    prizePool: 1760
  })
};
</script>