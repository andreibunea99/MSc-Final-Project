import os
import subprocess
from flask import Flask, jsonify, request, send_file, send_from_directory
from flask_cors import CORS
import cv2
import shutil
import base64

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

@app.route('/files/<email>/<path:file_path>')
def serve_file(email, file_path):
    DB = r'E:\Final_Project\repo\MSc-Final-Project\database'
    user_directory = os.path.join(DB, email)
    return send_from_directory(user_directory, file_path)

@app.route('/models/<email>', methods=['GET'])
def get_user_models(email):
    user_directory = os.path.join(DATABASE_FOLDER, email)
    if not os.path.exists(user_directory):
        return 'User directory not found', 404

    models = []
    for model_directory_name in os.listdir(user_directory):
        model_directory_path = os.path.join(user_directory, model_directory_name)
        if os.path.isdir(model_directory_path):
            model = {}

            # Get the file paths
            model['obj'] = f"/files/{email}/{os.path.join(model_directory_name, 'texturedMesh.obj')}"
            model['mtl'] = f"/files/{email}/{os.path.join(model_directory_name, 'texturedMesh.mtl')}"
            model['preview'] = f"/files/{email}/{os.path.join(model_directory_name, 'preview.jpg')}"

            textures = []
            for file_name in os.listdir(model_directory_path):
                if file_name.endswith('.png'):
                    texture_path = os.path.join(model_directory_path, file_name)
                    textures.append({
                        'filename': file_name,
                        'path': f"/files/{email}/{texture_path}"
                    })
            model['textures'] = textures

            # Get the name of the model (name of the directory)
            model['name'] = model_directory_name

            models.append(model)

    return jsonify(models)


@app.route('/previews/<email>', methods=['GET'])
def get_user_previews(email):
    user_directory = os.path.join(DATABASE_FOLDER, email)
    if not os.path.exists(user_directory):
        return 'User directory not found', 404

    models = []
    for model_directory_name in os.listdir(user_directory):
        model_directory_path = os.path.join(user_directory, model_directory_name)
        if os.path.isdir(model_directory_path):
            model = {}

            # Get the file paths
            model['preview'] = f"/files/{email}/{os.path.join(model_directory_name, 'preview.jpg')}"
            # Get the name of the model (name of the directory)
            model['name'] = model_directory_name

            models.append(model)

    return jsonify(models)


@app.route('/model/<model_name>/<email>', methods=['GET'])
def get_user_model(model_name, email):
    user_directory = os.path.join(DATABASE_FOLDER, email)
    model_directory_path = os.path.join(user_directory, model_name)
    if not os.path.exists(user_directory):
        return 'User directory not found', 404

    model = {}
    if os.path.isdir(model_directory_path):

        # Get the file paths
        model['obj'] = f"/files/{email}/{os.path.join(model_name, 'texturedMesh.obj')}"
        model['mtl'] = f"/files/{email}/{os.path.join(model_name, 'texturedMesh.mtl')}"
        model['preview'] = f"/files/{email}/{os.path.join(model_name, 'preview.jpg')}"

        textures = []
        for file_name in os.listdir(model_directory_path):
            if file_name.endswith('.png'):
                texture_path = os.path.join(model_directory_path, file_name)
                textures.append({
                    'filename': file_name,
                    'path': f"/files/{email}/{texture_path}"
                })
        model['textures'] = textures

        # Get the name of the model (name of the directory)
        model['name'] = model_name

    return jsonify(model)


@app.route('/edit/model', methods=['POST'])
def rename_model():
    user_email = request.form.get('email')
    model_name = request.form.get('model_name')
    new_name = request.form.get('new_name')

    user_directory = os.path.join(DATABASE_FOLDER, user_email)
    model_directory_path = os.path.join(user_directory, model_name)
    new_directory_path = os.path.join(user_directory, new_name)
    print(user_email, model_name, new_name)

    if not os.path.exists(user_directory) or not os.path.exists(model_directory_path):
        return 'User or model directory not found', 404

    if os.path.exists(new_directory_path):
        return 'New name already exists', 409

    try:
        os.rename(model_directory_path, new_directory_path)

        preview = request.files.get('preview')
        if preview:
            preview_path = os.path.join(new_directory_path, 'preview.jpg')
            preview.save(preview_path)

        return 'Model renamed successfully'
    except Exception as e:
        return f'Error renaming model: {str(e)}', 500



if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
