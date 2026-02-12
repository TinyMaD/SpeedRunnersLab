<template>
  <v-container class="comment-section pa-0" fluid>
    <v-card flat tile :color="$vuetify.theme.dark ? 'rgba(30,30,30,1)' : 'rgba(255,255,255,1)'">
      <v-card-title class="text-subtitle-1 font-weight-bold pb-2 pt-2">
        <v-icon left small>mdi-comment-text-multiple-outline</v-icon>
        {{ $t('comment.title') }}
        <span v-if="total > 0" class="ml-1 text-body-2 grey--text">({{ total }})</span>
      </v-card-title>

      <v-card-text>
        <!-- Post comment box -->
        <div v-if="isLoggedIn" class="comment-section__post mb-4">
          <div class="d-flex align-start" style="gap: 12px">
            <v-avatar size="36">
              <v-img v-if="userAvatar" :src="userAvatar" />
              <v-icon v-else>mdi-account-circle</v-icon>
            </v-avatar>
            <div style="flex: 1">
              <v-textarea
                v-model="newComment"
                :placeholder="$t('comment.placeholder')"
                rows="2"
                auto-grow
                outlined
                dense
                hide-details
                counter="2000"
                :maxlength="2000"
              />
              <div class="d-flex justify-end mt-2">
                <v-btn
                  color="primary"
                  small
                  :disabled="!newComment.trim()"
                  :loading="posting"
                  @click="postComment"
                >{{ $t('comment.submit') }}</v-btn>
              </div>
            </div>
          </div>
        </div>
        <div v-else class="text-center py-2 mb-4 grey--text text-body-2">
          <v-icon small class="mr-1">mdi-information-outline</v-icon>
          {{ $t('comment.loginToComment') }}
        </div>

        <v-divider class="mb-4" />

        <!-- Loading state -->
        <div v-if="loading" class="text-center py-6">
          <v-progress-circular indeterminate color="primary" size="32" />
        </div>

        <!-- Comments list -->
        <div v-else-if="comments.length > 0">
          <comment-item
            v-for="comment in comments"
            :key="comment.id"
            :comment="comment"
            :is-logged-in="isLoggedIn"
            :current-user="currentUser"
            @reply-posted="onReplyPosted"
            @comment-deleted="onCommentDeleted"
          />

          <!-- Pagination -->
          <div v-if="totalPages > 1" class="d-flex justify-center mt-4">
            <v-pagination
              v-model="pageNo"
              :length="totalPages"
              :total-visible="5"
              circle
              @input="loadComments"
            />
          </div>
        </div>

        <!-- Empty state -->
        <div v-else class="text-center py-6 grey--text text-body-2">
          <v-icon large class="mb-2" color="grey lighten-1">mdi-comment-outline</v-icon>
          <br>
          {{ $t('comment.noComments') }}
        </div>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script>
import CommentItem from "./CommentItem";
import { getCommentList, addComment } from "@/api/comment";
import { getToken } from "@/utils/auth";
import { mapGetters } from "vuex";

export default {
  name: "CommentSection",
  components: { CommentItem },
  data() {
    return {
      comments: [],
      total: 0,
      pageNo: 1,
      pageSize: 10,
      loading: false,
      newComment: "",
      posting: false
    };
  },
  computed: {
    ...mapGetters(["avatar", "steamId"]),
    isLoggedIn() {
      return !!getToken();
    },
    currentUser() {
      return this.steamId || "";
    },
    userAvatar() {
      return this.avatar || "";
    },
    pagePath() {
      return this.$route.path;
    },
    totalPages() {
      return Math.ceil(this.total / this.pageSize);
    }
  },
  watch: {
    "$route.path"() {
      this.pageNo = 1;
      this.loadComments();
    }
  },
  mounted() {
    this.loadComments();
  },
  methods: {
    async loadComments() {
      this.loading = true;
      try {
        const { data } = await getCommentList({
          pagePath: this.pagePath,
          pageNo: this.pageNo,
          pageSize: this.pageSize
        });
        this.comments = data.list || [];
        this.total = data.total || 0;
      } catch (e) {
        // error handled by request interceptor
      } finally {
        this.loading = false;
      }
    },
    async postComment() {
      if (!this.newComment.trim()) return;
      this.posting = true;
      try {
        await addComment({
          pagePath: this.pagePath,
          content: this.newComment.trim()
        });
        this.newComment = "";
        this.pageNo = 1;
        await this.loadComments();
      } catch (e) {
        // error handled by request interceptor
      } finally {
        this.posting = false;
      }
    },
    onReplyPosted() {
      // Refresh to update reply counts
      this.loadComments();
    },
    onCommentDeleted() {
      this.loadComments();
    }
  }
};
</script>

<style scoped>
.comment-section {
  max-width: 900px;
  margin: 0 auto;
  padding: 0 16px 24px;
}

/* 当作为侧边栏使用时，移除居中样式 */
:global(.comment-section-side) .comment-section,
.comment-section-side .comment-section {
  max-width: 100%;
  margin: 0;
  padding: 0;
}
</style>