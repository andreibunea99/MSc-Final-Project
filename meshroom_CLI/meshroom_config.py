
# configs = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]

configs = [
    {
        '--defaultFieldOfView': '45',
        '--allowSingleView': '1',  #nu e
    },  # 1
    {
        '--forceCpuExtraction': '1',
    },  # 2
    {}, # 3
    {
        '--knownPosesGeometricErrorMax': '5',
        '--describerTypes': 'sift', # dspsift
        '--photometricMatchingMethod':  'ANN_L2',
        '--geometricEstimator': 'acransac',
        '--geometricFilterType': 'fundamental_matrix',
        '--distanceRatio': '0.8',
        '--maxIteration': '2048',
        '--geometricError': '0.0',
        '--maxMatches': '0',
        '--savePutativeMatches': 'False',
        '--guidedMatching': 'False',
        '--matchFromKnownCameraPoses': 'False',
        '--exportDebugFiles': 'True',
    },  # 4
    {}, # 5
    {}, # 6
    {
        '--downscale': '2',
    }, # 7
    {}, # 8
    {
        '--maxInputPoints': '50000000',
        '--maxPoints': '1000000',
    }, # 9
    {
        '--keepLargestMeshOnly': 'True',
    }, # 10
    {
        '--simplificationFactor': '0.8',
        '--maxVertices': '15000',
    },  # 11
    {
        '--simplificationFactor': '0.8',
        '--maxVertices': '15000',
    },  # 12
    {
        '--textureSide': '4096',
        '--downscale': '4',
        '--unwrapMethod': 'Basic',
    }

]

configs[1] = {

}