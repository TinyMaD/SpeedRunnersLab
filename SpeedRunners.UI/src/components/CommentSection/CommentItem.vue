<template>
  <div class="comment-item">
    <div class="comment-item__main">
      <v-avatar size="36" class="comment-item__avatar">
        <v-img v-if="comment.avatarS" :src="comment.avatarS" />
        <v-icon v-else>mdi-account-circle</v-icon>
      </v-avatar>
      <div class="comment-item__body">
        <div class="comment-item__header">
          <span class="comment-item__name">{{ comment.personaName || comment.platformID }}</span>
          <span v-if="comment.replyToPersonaName" class="comment-item__reply-to">
            {{ $t('comment.reply') }} <strong>@{{ comment.replyToPersonaName }}</strong>
          </span>
          <span class="comment-item__time">{{ formatTime(comment.createTime) }}</span>
        </div>
        <div class="comment-item__content">{{ comment.content }}</div>
        <div class="comment-item__actions">
          <v-btn
            text
            x-small
            :color="liked ? 'red' : ''"
            :disabled="!isLoggedIn"
            :loading="liking"
            @click="handleLike"
          >
            <v-icon x-small class="mr-1 mt-1">{{ liked ? 'mdi-heart' : 'mdi-heart-outline' }}</v-icon>
            {{ likeCount > 0 ? likeCount : $t('comment.like') }}
          </v-btn>
          <v-btn v-if="isLoggedIn" text x-small @click="showReplyBox = !showReplyBox">
            <v-icon x-small class="mr-1 mt-1">mdi-reply</v-icon>{{ $t('comment.reply') }}
          </v-btn>
          <v-btn
            v-if="canDelete"
            text
            x-small
            color="error"
            @click="handleDelete"
          >
            <v-icon x-small class="mr-1 mt-1">mdi-delete-outline</v-icon>{{ $t('comment.delete') }}
          </v-btn>
        </div>

        <!-- Reply input box -->
        <div v-if="showReplyBox" class="comment-item__reply-box mt-2">
          <v-textarea
            v-model="replyContent"
            :placeholder="$t('comment.replyPlaceholder', [comment.personaName || comment.platformID])"
            rows="2"
            auto-grow
            outlined
            dense
            hide-details
            counter="2000"
            :maxlength="2000"
          />
          <div class="d-flex justify-end mt-1">
            <v-btn small text @click="showReplyBox = false">{{ $t('common.cancel') }}</v-btn>
            <v-btn
              small
              color="primary"
              :disabled="!replyContent.trim()"
              :loading="submitting"
              @click="submitReply"
            >{{ $t('comment.submit') }}</v-btn>
          </div>
        </div>

        <!-- Replies toggle for top-level comments -->
        <div v-if="!comment.parentID && comment.replyCount > 0" class="mt-2">
          <v-btn text x-small color="primary" @click="toggleReplies">
            <v-icon x-small left>{{ showReplies ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
            {{ showReplies ? $t('comment.hideReplies') : $t('comment.viewReplies', [comment.replyCount]) }}
          </v-btn>
        </div>

        <!-- Replies list -->
        <div v-if="showReplies && replies.length > 0" class="comment-item__replies" :style="{ borderColor: $vuetify.theme.dark ? 'rgba(255,255,255,0.15)' : 'rgba(0,0,0,0.12)' }">
          <comment-item
            v-for="reply in replies"
            :key="reply.id"
            :comment="reply"
            :is-logged-in="isLoggedIn"
            :current-user="currentUser"
            @reply-posted="onReplyPosted"
            @comment-deleted="onCommentDeleted"
          />
          <v-btn
            v-if="hasMoreReplies"
            text
            x-small
            color="primary"
            :loading="loadingReplies"
            @click="loadMoreReplies"
          >{{ $t('comment.loadMore') }}</v-btn>
        </div>
      </div>
    </div>

    <!-- Delete confirmation dialog -->
    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="text-h6">{{ $t('common.notice') }}</v-card-title>
        <v-card-text>
          {{ $t('comment.deleteConfirm') }}
          <br>
          <small class="grey--text">{{ $t('comment.deleteInfo') }}</small>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="deleteDialog = false">{{ $t('common.cancel') }}</v-btn>
          <v-btn color="error" text :loading="deleting" @click="confirmDelete">{{ $t('common.confirm') }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script>
import { addComment, deleteComment, getCommentList, toggleLike } from "@/api/comment";

export default {
  name: "CommentItem",
  props: {
    comment: { type: Object, required: true },
    isLoggedIn: { type: Boolean, default: false },
    currentUser: { type: String, default: "" }
  },
  data() {
    return {
      showReplyBox: false,
      replyContent: "",
      submitting: false,
      showReplies: false,
      replies: [],
      replyPageNo: 1,
      replyTotal: 0,
      loadingReplies: false,
      deleteDialog: false,
      deleting: false,
      liked: !!this.comment.isLiked,
      likeCount: this.comment.likeCount || 0,
      liking: false
    };
  },
  computed: {
    canDelete() {
      if (!this.isLoggedIn) return false;
      return this.comment.platformID === this.currentUser || this.currentUser === "76561198062688821";
    },
    hasMoreReplies() {
      return this.replies.length < this.replyTotal;
    }
  },
  methods: {
    formatTime(time) {
      if (!time) return "";
      const date = new Date(time);
      const now = new Date();
      const diff = now - date;
      const minutes = Math.floor(diff / 60000);
      const hours = Math.floor(diff / 3600000);
      const days = Math.floor(diff / 86400000);
      const years = Math.floor(days / 365);

      if (minutes < 1) return this.$t("comment.time.justNow");
      if (minutes < 60) return this.$t("comment.time.minutesAgo", [minutes]);
      if (hours < 24) return this.$t("comment.time.hoursAgo", [hours]);
      if (days < 30) return this.$t("comment.time.daysAgo", [days]);
      if (years >= 1) return this.$t("comment.time.yearsAgo", [years]);
      return date.toLocaleDateString();
    },
    async submitReply() {
      if (!this.replyContent.trim()) return;
      this.submitting = true;
      try {
        const parentID = this.comment.parentID || this.comment.id;
        await addComment({
          pagePath: this.comment.pagePath,
          parentID: parentID,
          replyToPlatformID: this.comment.platformID,
          content: this.replyContent.trim()
        });
        this.replyContent = "";
        this.showReplyBox = false;
        this.$emit("reply-posted", parentID);
        // If this is a top-level comment, refresh replies
        if (!this.comment.parentID) {
          await this.loadReplies();
          this.showReplies = true;
        }
      } catch (e) {
        // error handled by request interceptor
      } finally {
        this.submitting = false;
      }
    },
    async toggleReplies() {
      if (this.showReplies) {
        this.showReplies = false;
        return;
      }
      if (this.replies.length === 0) {
        await this.loadReplies();
      }
      this.showReplies = true;
    },
    async loadReplies() {
      this.loadingReplies = true;
      this.replyPageNo = 1;
      try {
        const { data } = await getCommentList({
          pagePath: this.comment.pagePath,
          parentID: this.comment.id,
          pageNo: 1,
          pageSize: 10
        });
        this.replies = data.list || [];
        this.replyTotal = data.total || 0;
      } catch (e) {
        // error handled by request interceptor
      } finally {
        this.loadingReplies = false;
      }
    },
    async loadMoreReplies() {
      this.loadingReplies = true;
      this.replyPageNo++;
      try {
        const { data } = await getCommentList({
          pagePath: this.comment.pagePath,
          parentID: this.comment.id,
          pageNo: this.replyPageNo,
          pageSize: 10
        });
        this.replies = this.replies.concat(data.list || []);
      } catch (e) {
        this.replyPageNo--;
      } finally {
        this.loadingReplies = false;
      }
    },
    handleDelete() {
      this.deleteDialog = true;
    },
    async handleLike() {
      if (!this.isLoggedIn) return;
      this.liking = true;
      try {
        const { data } = await toggleLike(this.comment.id);
        this.liked = !this.liked;
        this.likeCount = data;
      } catch (e) {
        // error handled by request interceptor
      } finally {
        this.liking = false;
      }
    },
    async confirmDelete() {
      this.deleting = true;
      try {
        await deleteComment(this.comment.id);
        this.deleteDialog = false;
        this.$emit("comment-deleted", this.comment.id);
      } catch (e) {
        // error handled by request interceptor
      } finally {
        this.deleting = false;
      }
    },
    onReplyPosted(parentID) {
      this.$emit("reply-posted", parentID);
      this.loadReplies();
    },
    onCommentDeleted(commentID) {
      this.replies = this.replies.filter(r => r.id !== commentID);
      this.replyTotal = Math.max(0, this.replyTotal - 1);
      this.$emit("comment-deleted", commentID);
    }
  }
};
</script>

<style scoped>
.comment-item {
  padding: 8px 0;
}
.comment-item__main {
  display: flex;
  gap: 12px;
}
.comment-item__avatar {
  flex-shrink: 0;
}
.comment-item__body {
  flex: 1;
  min-width: 0;
}
.comment-item__header {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 4px;
}
.comment-item__name {
  font-weight: 600;
  font-size: 13px;
}
.comment-item__reply-to {
  font-size: 12px;
  opacity: 0.7;
}
.comment-item__time {
  font-size: 12px;
  opacity: 0.5;
}
.comment-item__content {
  font-size: 14px;
  line-height: 1.5;
  word-break: break-word;
  white-space: pre-wrap;
}
.comment-item__actions {
  margin-top: 2px;
  margin-left: -8px;
}
.comment-item__replies {
  margin-top: 8px;
  padding-left: 4px;
  border-left: 2px solid rgba(128, 128, 128, 0.2);
}
</style>
