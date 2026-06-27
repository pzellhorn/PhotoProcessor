from worker_core.consumer import BaseConsumer

class FaceRecognitionWorker(BaseConsumer):
    def process(self, job: dict) -> None:
        print(f"fingerprinting {job['mediaUri']}")
         

if __name__ == "__main__":
    FaceRecognitionWorker(queue="jobs").run()