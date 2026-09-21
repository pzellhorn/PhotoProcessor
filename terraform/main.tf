terraform {
  required_version = ">= 1.5"

  required_providers {
    minio = {
      source  = "aminueza/minio"
      version = "~> 3.0"
    }
  }
}

provider "minio" {
  minio_server   = var.minio_server
  minio_user     = var.minio_user
  minio_password = var.minio_password
  minio_ssl      = false
}

resource "minio_s3_bucket" "media" {
  bucket = var.media_bucket
}
 
resource "minio_s3_bucket_notification" "media" {
  bucket = minio_s3_bucket.media.bucket

  queue {
    id            = "photos"
    queue_arn     = "arn:minio:sqs::PRIMARY:amqp"
    events        = ["s3:ObjectCreated:*"]
    filter_prefix = "photos/"
  }

  queue {
    id            = "videos"
    queue_arn     = "arn:minio:sqs::SECONDARY:amqp"
    events        = ["s3:ObjectCreated:*"]
    filter_prefix = "videos/raw/"
  }
}
