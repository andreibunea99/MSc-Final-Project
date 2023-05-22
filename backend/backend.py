import os
import subprocess
from flask import Flask, jsonify, request, send_file
from flask_cors import CORS
import cv2
import shutil

app = Flask(__name__)
CORS(app)

UPLOAD_FOLDER = os.path.join(os.getcwd(), '../image_dataset')
VIDEO_FOLDER = os.path.join(os.getcwd(), '../video')
OUTPUT_FOLDER = os.path.join(os.getcwd(), '../output')
DATABASE_FOLDER = os.path.join(os.getcwd(), '../database')
app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
app.config['VIDEO_FOLDER'] = VIDEO_FOLDER
app.config['OUTPUT_FOLDER'] = OUTPUT_FOLDER
app.config['DATABASE_FOLDER'] = DATABASE_FOLDER


def save_images(file_storage, save_folder):
    # Delete files within each directory in the 'outputs' folder
    for root, dirs, files in os.walk(app.config['OUTPUT_FOLDER']):
        for file_name in files:
            file_path = os.path.join(root, file_name)
            os.remove(file_path)

    # Delete the directories within the 'outputs' folder
    for root, dirs, files in os.walk(app.config['OUTPUT_FOLDER'], topdown=False):
        for dir_name in dirs:
            dir_path = os.path.join(root, dir_name)
            shutil.rmtree(dir_path)

    os.makedirs(save_folder, exist_ok=True)
    filename = file_storage.filename
    filepath = os.path.join(save_folder, filename)
    file_storage.save(filepath)

    return filepath

def extract_frames(video_path, save_folder, frame_interval=1):
    os.makedirs(save_folder, exist_ok=True)
    save_path_template = os.path.join(save_folder, 'frame{}.jpg')

    video_capture = cv2.VideoCapture(video_path)
    frame_count = int(video_capture.get(cv2.CAP_PROP_FRAME_COUNT))

    for frame_index in range(0, frame_count, frame_interval):
        video_capture.set(cv2.CAP_PROP_POS_FRAMES, frame_index)
        success, frame = video_capture.read()
        if success:
            save_path = save_path_template.format(frame_index)
            cv2.imwrite(save_path, frame)

    video_capture.release()

def run_meshcli_script(user_email, preview, name):
    script_path = os.path.join(os.getcwd(), '..', 'meshroom_CLI', 'Run.bat')
    subprocess.call(script_path, shell=True)

    user_directory = os.path.join(DATABASE_FOLDER, user_email)
    if not os.path.exists(user_directory):
        os.makedirs(user_directory)

    model_directory_name = name
    model_directory_path = os.path.join(user_directory, model_directory_name)

    # Create the model directory inside the user's directory
    os.makedirs(model_directory_path, exist_ok=True)

    # Copy the contents of the 13_Texturing directory to the model directory
    source_directory = os.path.join(os.getcwd(), '..', 'output', '13_Texturing')
    for item in os.listdir(source_directory):
        source_item_path = os.path.join(source_directory, item)
        destination_item_path = os.path.join(model_directory_path, item)
        shutil.copy2(source_item_path, destination_item_path)
    
    # Save the preview as preview.jpg inside the model directory the preview is given as argument of the function
    preview_path = os.path.join(model_directory_path, 'preview.jpg')
    preview.save(preview_path)



@app.route('/upload/images', methods=['POST'])
def upload_images():
    if 'images' not in request.files:
        return 'No images found', 400

    images = request.files.getlist('images')
    user_email = request.form.get('email')
    preview = request.files.get('preview')
    name = request.form.get('name')
    
    # Delete all files from the image dataset folder
    for file_name in os.listdir(app.config['UPLOAD_FOLDER']):
        file_path = os.path.join(app.config['UPLOAD_FOLDER'], file_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

    for image in images:
        save_images(image, app.config['UPLOAD_FOLDER'])

    print('Running meshroom script...')
    run_meshcli_script(user_email, preview, name)

    return 'Images uploaded successfully'

@app.route('/upload/video', methods=['POST'])
def upload_video():
    if 'video' not in request.files:
        return 'No video found', 400

    video = request.files['video']
    video_path = os.path.join(app.config['VIDEO_FOLDER'], 'video.mp4')

    user_email = request.form.get('email')
    preview = request.form.get('preview')
    name = request.form.get('name')

    video.save(video_path)

    # Delete all files from the image dataset folder
    for file_name in os.listdir(app.config['UPLOAD_FOLDER']):
        file_path = os.path.join(app.config['UPLOAD_FOLDER'], file_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

    # Extract frames from the video
    extract_frames(video_path, app.config['UPLOAD_FOLDER'], frame_interval=25)

    # Delete the video file
    os.remove(video_path)

    # # Run the meshcli script
    print('Running meshroom script...')
    run_meshcli_script(user_email, preview, name)

    return 'Video uploaded successfully'


@app.route('/models/<email>', methods=['GET'])
def get_user_models(email):
    # Retrieve the models from app.config['DATABASE_FOLDER']/email directory and return .png files as list, the .obj file and .mtl file
    user_directory = os.path.join(DATABASE_FOLDER, email)
    if not os.path.exists(user_directory):
        os.makedirs(user_directory)

    # Count the number of directories inside the user's directory
    existing_models = [name for name in os.listdir(user_directory) if os.path.isdir(os.path.join(user_directory, name))]
    model_count = len(existing_models)

    models = []
    for i in range(0, model_count):  
        model = {}
        model['id'] = i + 1

        # There are multiple files, so we need to get all of them
        model_directory_name = f"model_{i + 1}"
        model_directory_path = os.path.join(user_directory, model_directory_name)

        # Get the .png files and return them as a list
        model['textures'] = []
        for file_name in os.listdir(model_directory_path):
            if file_name.endswith('.png'):
                model['textures'].append(file_name)

        # Get the .obj file
        model['obj'] = f'model_{i}.obj'

        # Get the .mtl file
        model['mtl'] = f'model_{i}.mtl'

        models.append(model)
    
    return jsonify(models)



if __name__ == '__main__':
    app.run()
