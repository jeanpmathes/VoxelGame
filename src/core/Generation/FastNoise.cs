// <copyright file="FastNoise.cs" company="Auburns">
//     MIT License
//
//Copyright(c) 2016 Jordan Peck
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
// </copyright>
// <author>Jordan Peck</author>

#pragma warning disable
// ReSharper disable all

// Uncomment the line below to swap all the inputs/outputs/calculations of FastNoise to doubles instead of floats
//#define FN_USE_DOUBLES

#if FN_USE_DOUBLES
using FN_DECIMAL = System.Double;
#else

using FN_DECIMAL = System.Single;
#endif
using System;
using System.Runtime.CompilerServices;

namespace VoxelGame.Core.Generation
{
    public class FastNoise
    {
        public enum CellularDistanceFunction
        {
            Euclidean,
            Manhattan,
            Natural
        };

        public enum CellularReturnType
        {
            CellValue,
            NoiseLookup,
            Distance,
            Distance2,
            Distance2Add,
            Distance2Sub,
            Distance2Mul,
            Distance2Div
        };

        public enum FractalType
        {
            FBM,
            Billow,
            RigidMulti
        };

        public enum Interp
        {
            Linear,
            Hermite,
            Quintic
        };

        public enum NoiseType
        {
            Value,
            ValueFractal,
            Perlin,
            PerlinFractal,
            Simplex,
            SimplexFractal,
            Cellular,
            WhiteNoise,
            Cubic,
            CubicFractal
        };

        private const Int16 FN_INLINE = 256; //(Int16)MethodImplOptions.AggressiveInlining;
        private const int FN_CELLULAR_INDEX_MAX = 3;

        // Hashing
        private const int X_PRIME = 1619;

        private const int Y_PRIME = 31337;
        private const int Z_PRIME = 6971;
        private const int W_PRIME = 1013;

        private const FN_DECIMAL F3 = (FN_DECIMAL) (1.0 / 3.0);
        private const FN_DECIMAL G3 = (FN_DECIMAL) (1.0 / 6.0);
        private const FN_DECIMAL G33 = G3 * 3 - 1;

        private const FN_DECIMAL F2 = (FN_DECIMAL) (1.0 / 2.0);
        private const FN_DECIMAL G2 = (FN_DECIMAL) (1.0 / 4.0);

        private const FN_DECIMAL F4 = (FN_DECIMAL) ((2.23606797 - 1.0) / 4.0);
        private const FN_DECIMAL G4 = (FN_DECIMAL) ((5.0 - 2.23606797) / 20.0);

        private const FN_DECIMAL CUBIC_3D_BOUNDING = 1 / (FN_DECIMAL) (1.5 * 1.5 * 1.5);

        private const FN_DECIMAL CUBIC_2D_BOUNDING = 1 / (FN_DECIMAL) (1.5 * 1.5);

        private static readonly Float2[] GRAD_2D =
        {
            new Float2(x: -1, y: -1), new Float2(x: 1, y: -1), new Float2(x: -1, y: 1), new Float2(x: 1, y: 1),
            new Float2(x: 0, y: -1), new Float2(x: -1, y: 0), new Float2(x: 0, y: 1), new Float2(x: 1, y: 0)
        };

        private static readonly Float3[] GRAD_3D =
        {
            new Float3(x: 1, y: 1, z: 0), new Float3(x: -1, y: 1, z: 0), new Float3(x: 1, y: -1, z: 0),
            new Float3(x: -1, y: -1, z: 0),
            new Float3(x: 1, y: 0, z: 1), new Float3(x: -1, y: 0, z: 1), new Float3(x: 1, y: 0, z: -1),
            new Float3(x: -1, y: 0, z: -1),
            new Float3(x: 0, y: 1, z: 1), new Float3(x: 0, y: -1, z: 1), new Float3(x: 0, y: 1, z: -1),
            new Float3(x: 0, y: -1, z: -1),
            new Float3(x: 1, y: 1, z: 0), new Float3(x: 0, y: -1, z: 1), new Float3(x: -1, y: 1, z: 0),
            new Float3(x: 0, y: -1, z: -1)
        };

        private static readonly Float2[] CELL_2D =
        {
            new Float2(x: -0.2700222198f, y: -0.9628540911f), new Float2(x: 0.3863092627f, y: -0.9223693152f),
            new Float2(x: 0.04444859006f, y: -0.999011673f), new Float2(x: -0.5992523158f, y: -0.8005602176f),
            new Float2(x: -0.7819280288f, y: 0.6233687174f), new Float2(x: 0.9464672271f, y: 0.3227999196f),
            new Float2(x: -0.6514146797f, y: -0.7587218957f), new Float2(x: 0.9378472289f, y: 0.347048376f),
            new Float2(x: -0.8497875957f, y: -0.5271252623f), new Float2(x: -0.879042592f, y: 0.4767432447f),
            new Float2(x: -0.892300288f, y: -0.4514423508f), new Float2(x: -0.379844434f, y: -0.9250503802f),
            new Float2(x: -0.9951650832f, y: 0.0982163789f), new Float2(x: 0.7724397808f, y: -0.6350880136f),
            new Float2(x: 0.7573283322f, y: -0.6530343002f), new Float2(x: -0.9928004525f, y: -0.119780055f),
            new Float2(x: -0.0532665713f, y: 0.9985803285f), new Float2(x: 0.9754253726f, y: -0.2203300762f),
            new Float2(x: -0.7665018163f, y: 0.6422421394f), new Float2(x: 0.991636706f, y: 0.1290606184f),
            new Float2(x: -0.994696838f, y: 0.1028503788f), new Float2(x: -0.5379205513f, y: -0.84299554f),
            new Float2(x: 0.5022815471f, y: -0.8647041387f), new Float2(x: 0.4559821461f, y: -0.8899889226f),
            new Float2(x: -0.8659131224f, y: -0.5001944266f), new Float2(x: 0.0879458407f, y: -0.9961252577f),
            new Float2(x: -0.5051684983f, y: 0.8630207346f), new Float2(x: 0.7753185226f, y: -0.6315704146f),
            new Float2(x: -0.6921944612f, y: 0.7217110418f), new Float2(x: -0.5191659449f, y: -0.8546734591f),
            new Float2(x: 0.8978622882f, y: -0.4402764035f), new Float2(x: -0.1706774107f, y: 0.9853269617f),
            new Float2(x: -0.9353430106f, y: -0.3537420705f), new Float2(x: -0.9992404798f, y: 0.03896746794f),
            new Float2(x: -0.2882064021f, y: -0.9575683108f), new Float2(x: -0.9663811329f, y: 0.2571137995f),
            new Float2(x: -0.8759714238f, y: -0.4823630009f), new Float2(x: -0.8303123018f, y: -0.5572983775f),
            new Float2(x: 0.05110133755f, y: -0.9986934731f), new Float2(x: -0.8558373281f, y: -0.5172450752f),
            new Float2(x: 0.09887025282f, y: 0.9951003332f), new Float2(x: 0.9189016087f, y: 0.3944867976f),
            new Float2(x: -0.2439375892f, y: -0.9697909324f), new Float2(x: -0.8121409387f, y: -0.5834613061f),
            new Float2(x: -0.9910431363f, y: 0.1335421355f), new Float2(x: 0.8492423985f, y: -0.5280031709f),
            new Float2(x: -0.9717838994f, y: -0.2358729591f), new Float2(x: 0.9949457207f, y: 0.1004142068f),
            new Float2(x: 0.6241065508f, y: -0.7813392434f), new Float2(x: 0.662910307f, y: 0.7486988212f),
            new Float2(x: -0.7197418176f, y: 0.6942418282f), new Float2(x: -0.8143370775f, y: -0.5803922158f),
            new Float2(x: 0.104521054f, y: -0.9945226741f), new Float2(x: -0.1065926113f, y: -0.9943027784f),
            new Float2(x: 0.445799684f, y: -0.8951327509f), new Float2(x: 0.105547406f, y: 0.9944142724f),
            new Float2(x: -0.992790267f, y: 0.1198644477f), new Float2(x: -0.8334366408f, y: 0.552615025f),
            new Float2(x: 0.9115561563f, y: -0.4111755999f), new Float2(x: 0.8285544909f, y: -0.5599084351f),
            new Float2(x: 0.7217097654f, y: -0.6921957921f), new Float2(x: 0.4940492677f, y: -0.8694339084f),
            new Float2(x: -0.3652321272f, y: -0.9309164803f), new Float2(x: -0.9696606758f, y: 0.2444548501f),
            new Float2(x: 0.08925509731f, y: -0.996008799f), new Float2(x: 0.5354071276f, y: -0.8445941083f),
            new Float2(x: -0.1053576186f, y: 0.9944343981f), new Float2(x: -0.9890284586f, y: 0.1477251101f),
            new Float2(x: 0.004856104961f, y: 0.9999882091f), new Float2(x: 0.9885598478f, y: 0.1508291331f),
            new Float2(x: 0.9286129562f, y: -0.3710498316f), new Float2(x: -0.5832393863f, y: -0.8123003252f),
            new Float2(x: 0.3015207509f, y: 0.9534596146f), new Float2(x: -0.9575110528f, y: 0.2883965738f),
            new Float2(x: 0.9715802154f, y: -0.2367105511f), new Float2(x: 0.229981792f, y: 0.9731949318f),
            new Float2(x: 0.955763816f, y: -0.2941352207f), new Float2(x: 0.740956116f, y: 0.6715534485f),
            new Float2(x: -0.9971513787f, y: -0.07542630764f), new Float2(x: 0.6905710663f, y: -0.7232645452f),
            new Float2(x: -0.290713703f, y: -0.9568100872f), new Float2(x: 0.5912777791f, y: -0.8064679708f),
            new Float2(x: -0.9454592212f, y: -0.325740481f), new Float2(x: 0.6664455681f, y: 0.74555369f),
            new Float2(x: 0.6236134912f, y: 0.7817328275f), new Float2(x: 0.9126993851f, y: -0.4086316587f),
            new Float2(x: -0.8191762011f, y: 0.5735419353f), new Float2(x: -0.8812745759f, y: -0.4726046147f),
            new Float2(x: 0.9953313627f, y: 0.09651672651f), new Float2(x: 0.9855650846f, y: -0.1692969699f),
            new Float2(x: -0.8495980887f, y: 0.5274306472f), new Float2(x: 0.6174853946f, y: -0.7865823463f),
            new Float2(x: 0.8508156371f, y: 0.52546432f), new Float2(x: 0.9985032451f, y: -0.05469249926f),
            new Float2(x: 0.1971371563f, y: -0.9803759185f), new Float2(x: 0.6607855748f, y: -0.7505747292f),
            new Float2(x: -0.03097494063f, y: 0.9995201614f), new Float2(x: -0.6731660801f, y: 0.739491331f),
            new Float2(x: -0.7195018362f, y: -0.6944905383f), new Float2(x: 0.9727511689f, y: 0.2318515979f),
            new Float2(x: 0.9997059088f, y: -0.0242506907f), new Float2(x: 0.4421787429f, y: -0.8969269532f),
            new Float2(x: 0.9981350961f, y: -0.061043673f), new Float2(x: -0.9173660799f, y: -0.3980445648f),
            new Float2(x: -0.8150056635f, y: -0.5794529907f), new Float2(x: -0.8789331304f, y: 0.4769450202f),
            new Float2(x: 0.0158605829f, y: 0.999874213f), new Float2(x: -0.8095464474f, y: 0.5870558317f),
            new Float2(x: -0.9165898907f, y: -0.3998286786f), new Float2(x: -0.8023542565f, y: 0.5968480938f),
            new Float2(x: -0.5176737917f, y: 0.8555780767f), new Float2(x: -0.8154407307f, y: -0.5788405779f),
            new Float2(x: 0.4022010347f, y: -0.9155513791f), new Float2(x: -0.9052556868f, y: -0.4248672045f),
            new Float2(x: 0.7317445619f, y: 0.6815789728f), new Float2(x: -0.5647632201f, y: -0.8252529947f),
            new Float2(x: -0.8403276335f, y: -0.5420788397f), new Float2(x: -0.9314281527f, y: 0.363925262f),
            new Float2(x: 0.5238198472f, y: 0.8518290719f), new Float2(x: 0.7432803869f, y: -0.6689800195f),
            new Float2(x: -0.985371561f, y: -0.1704197369f), new Float2(x: 0.4601468731f, y: 0.88784281f),
            new Float2(x: 0.825855404f, y: 0.5638819483f), new Float2(x: 0.6182366099f, y: 0.7859920446f),
            new Float2(x: 0.8331502863f, y: -0.553046653f), new Float2(x: 0.1500307506f, y: 0.9886813308f),
            new Float2(x: -0.662330369f, y: -0.7492119075f), new Float2(x: -0.668598664f, y: 0.743623444f),
            new Float2(x: 0.7025606278f, y: 0.7116238924f), new Float2(x: -0.5419389763f, y: -0.8404178401f),
            new Float2(x: -0.3388616456f, y: 0.9408362159f), new Float2(x: 0.8331530315f, y: 0.5530425174f),
            new Float2(x: -0.2989720662f, y: -0.9542618632f), new Float2(x: 0.2638522993f, y: 0.9645630949f),
            new Float2(x: 0.124108739f, y: -0.9922686234f), new Float2(x: -0.7282649308f, y: -0.6852956957f),
            new Float2(x: 0.6962500149f, y: 0.7177993569f), new Float2(x: -0.9183535368f, y: 0.3957610156f),
            new Float2(x: -0.6326102274f, y: -0.7744703352f), new Float2(x: -0.9331891859f, y: -0.359385508f),
            new Float2(x: -0.1153779357f, y: -0.9933216659f), new Float2(x: 0.9514974788f, y: -0.3076565421f),
            new Float2(x: -0.08987977445f, y: -0.9959526224f), new Float2(x: 0.6678496916f, y: 0.7442961705f),
            new Float2(x: 0.7952400393f, y: -0.6062947138f), new Float2(x: -0.6462007402f, y: -0.7631674805f),
            new Float2(x: -0.2733598753f, y: 0.9619118351f), new Float2(x: 0.9669590226f, y: -0.254931851f),
            new Float2(x: -0.9792894595f, y: 0.2024651934f), new Float2(x: -0.5369502995f, y: -0.8436138784f),
            new Float2(x: -0.270036471f, y: -0.9628500944f), new Float2(x: -0.6400277131f, y: 0.7683518247f),
            new Float2(x: -0.7854537493f, y: -0.6189203566f), new Float2(x: 0.06005905383f, y: -0.9981948257f),
            new Float2(x: -0.02455770378f, y: 0.9996984141f), new Float2(x: -0.65983623f, y: 0.751409442f),
            new Float2(x: -0.6253894466f, y: -0.7803127835f), new Float2(x: -0.6210408851f, y: -0.7837781695f),
            new Float2(x: 0.8348888491f, y: 0.5504185768f), new Float2(x: -0.1592275245f, y: 0.9872419133f),
            new Float2(x: 0.8367622488f, y: 0.5475663786f), new Float2(x: -0.8675753916f, y: -0.4973056806f),
            new Float2(x: -0.2022662628f, y: -0.9793305667f), new Float2(x: 0.9399189937f, y: 0.3413975472f),
            new Float2(x: 0.9877404807f, y: -0.1561049093f), new Float2(x: -0.9034455656f, y: 0.4287028224f),
            new Float2(x: 0.1269804218f, y: -0.9919052235f), new Float2(x: -0.3819600854f, y: 0.924178821f),
            new Float2(x: 0.9754625894f, y: 0.2201652486f), new Float2(x: -0.3204015856f, y: -0.9472818081f),
            new Float2(x: -0.9874760884f, y: 0.1577687387f), new Float2(x: 0.02535348474f, y: -0.9996785487f),
            new Float2(x: 0.4835130794f, y: -0.8753371362f), new Float2(x: -0.2850799925f, y: -0.9585037287f),
            new Float2(x: -0.06805516006f, y: -0.99768156f), new Float2(x: -0.7885244045f, y: -0.6150034663f),
            new Float2(x: 0.3185392127f, y: -0.9479096845f), new Float2(x: 0.8880043089f, y: 0.4598351306f),
            new Float2(x: 0.6476921488f, y: -0.7619021462f), new Float2(x: 0.9820241299f, y: 0.1887554194f),
            new Float2(x: 0.9357275128f, y: -0.3527237187f), new Float2(x: -0.8894895414f, y: 0.4569555293f),
            new Float2(x: 0.7922791302f, y: 0.6101588153f), new Float2(x: 0.7483818261f, y: 0.6632681526f),
            new Float2(x: -0.7288929755f, y: -0.6846276581f), new Float2(x: 0.8729032783f, y: -0.4878932944f),
            new Float2(x: 0.8288345784f, y: 0.5594937369f), new Float2(x: 0.08074567077f, y: 0.9967347374f),
            new Float2(x: 0.9799148216f, y: -0.1994165048f), new Float2(x: -0.580730673f, y: -0.8140957471f),
            new Float2(x: -0.4700049791f, y: -0.8826637636f), new Float2(x: 0.2409492979f, y: 0.9705377045f),
            new Float2(x: 0.9437816757f, y: -0.3305694308f), new Float2(x: -0.8927998638f, y: -0.4504535528f),
            new Float2(x: -0.8069622304f, y: 0.5906030467f), new Float2(x: 0.06258973166f, y: 0.9980393407f),
            new Float2(x: -0.9312597469f, y: 0.3643559849f), new Float2(x: 0.5777449785f, y: 0.8162173362f),
            new Float2(x: -0.3360095855f, y: -0.941858566f), new Float2(x: 0.697932075f, y: -0.7161639607f),
            new Float2(x: -0.002008157227f, y: -0.9999979837f), new Float2(x: -0.1827294312f, y: -0.9831632392f),
            new Float2(x: -0.6523911722f, y: 0.7578824173f), new Float2(x: -0.4302626911f, y: -0.9027037258f),
            new Float2(x: -0.9985126289f, y: -0.05452091251f), new Float2(x: -0.01028102172f, y: -0.9999471489f),
            new Float2(x: -0.4946071129f, y: 0.8691166802f), new Float2(x: -0.2999350194f, y: 0.9539596344f),
            new Float2(x: 0.8165471961f, y: 0.5772786819f), new Float2(x: 0.2697460475f, y: 0.962931498f),
            new Float2(x: -0.7306287391f, y: -0.6827749597f), new Float2(x: -0.7590952064f, y: -0.6509796216f),
            new Float2(x: -0.907053853f, y: 0.4210146171f), new Float2(x: -0.5104861064f, y: -0.8598860013f),
            new Float2(x: 0.8613350597f, y: 0.5080373165f), new Float2(x: 0.5007881595f, y: -0.8655698812f),
            new Float2(x: -0.654158152f, y: 0.7563577938f), new Float2(x: -0.8382755311f, y: -0.545246856f),
            new Float2(x: 0.6940070834f, y: 0.7199681717f), new Float2(x: 0.06950936031f, y: 0.9975812994f),
            new Float2(x: 0.1702942185f, y: -0.9853932612f), new Float2(x: 0.2695973274f, y: 0.9629731466f),
            new Float2(x: 0.5519612192f, y: -0.8338697815f), new Float2(x: 0.225657487f, y: -0.9742067022f),
            new Float2(x: 0.4215262855f, y: -0.9068161835f), new Float2(x: 0.4881873305f, y: -0.8727388672f),
            new Float2(x: -0.3683854996f, y: -0.9296731273f), new Float2(x: -0.9825390578f, y: 0.1860564427f),
            new Float2(x: 0.81256471f, y: 0.5828709909f), new Float2(x: 0.3196460933f, y: -0.9475370046f),
            new Float2(x: 0.9570913859f, y: 0.2897862643f), new Float2(x: -0.6876655497f, y: -0.7260276109f),
            new Float2(x: -0.9988770922f, y: -0.047376731f), new Float2(x: -0.1250179027f, y: 0.992154486f),
            new Float2(x: -0.8280133617f, y: 0.560708367f), new Float2(x: 0.9324863769f, y: -0.3612051451f),
            new Float2(x: 0.6394653183f, y: 0.7688199442f), new Float2(x: -0.01623847064f, y: -0.9998681473f),
            new Float2(x: -0.9955014666f, y: -0.09474613458f), new Float2(x: -0.81453315f, y: 0.580117012f),
            new Float2(x: 0.4037327978f, y: -0.9148769469f), new Float2(x: 0.9944263371f, y: 0.1054336766f),
            new Float2(x: -0.1624711654f, y: 0.9867132919f), new Float2(x: -0.9949487814f, y: -0.100383875f),
            new Float2(x: -0.6995302564f, y: 0.7146029809f), new Float2(x: 0.5263414922f, y: -0.85027327f),
            new Float2(x: -0.5395221479f, y: 0.841971408f), new Float2(x: 0.6579370318f, y: 0.7530729462f),
            new Float2(x: 0.01426758847f, y: -0.9998982128f), new Float2(x: -0.6734383991f, y: 0.7392433447f),
            new Float2(x: 0.639412098f, y: -0.7688642071f), new Float2(x: 0.9211571421f, y: 0.3891908523f),
            new Float2(x: -0.146637214f, y: -0.9891903394f), new Float2(x: -0.782318098f, y: 0.6228791163f),
            new Float2(x: -0.5039610839f, y: -0.8637263605f), new Float2(x: -0.7743120191f, y: -0.6328039957f)
        };

        private static readonly Float3[] CELL_3D =
        {
            new Float3(x: -0.7292736885f, y: -0.6618439697f, z: 0.1735581948f),
            new Float3(x: 0.790292081f, y: -0.5480887466f, z: -0.2739291014f),
            new Float3(x: 0.7217578935f, y: 0.6226212466f, z: -0.3023380997f),
            new Float3(x: 0.565683137f, y: -0.8208298145f, z: -0.0790000257f),
            new Float3(x: 0.760049034f, y: -0.5555979497f, z: -0.3370999617f),
            new Float3(x: 0.3713945616f, y: 0.5011264475f, z: 0.7816254623f),
            new Float3(x: -0.1277062463f, y: -0.4254438999f, z: -0.8959289049f),
            new Float3(x: -0.2881560924f, y: -0.5815838982f, z: 0.7607405838f),
            new Float3(x: 0.5849561111f, y: -0.662820239f, z: -0.4674352136f),
            new Float3(x: 0.3307171178f, y: 0.0391653737f, z: 0.94291689f),
            new Float3(x: 0.8712121778f, y: -0.4113374369f, z: -0.2679381538f),
            new Float3(x: 0.580981015f, y: 0.7021915846f, z: 0.4115677815f),
            new Float3(x: 0.503756873f, y: 0.6330056931f, z: -0.5878203852f),
            new Float3(x: 0.4493712205f, y: 0.601390195f, z: 0.6606022552f),
            new Float3(x: -0.6878403724f, y: 0.09018890807f, z: -0.7202371714f),
            new Float3(x: -0.5958956522f, y: -0.6469350577f, z: 0.475797649f),
            new Float3(x: -0.5127052122f, y: 0.1946921978f, z: -0.8361987284f),
            new Float3(x: -0.9911507142f, y: -0.05410276466f, z: -0.1212153153f),
            new Float3(x: -0.2149721042f, y: 0.9720882117f, z: -0.09397607749f),
            new Float3(x: -0.7518650936f, y: -0.5428057603f, z: 0.3742469607f),
            new Float3(x: 0.5237068895f, y: 0.8516377189f, z: -0.02107817834f),
            new Float3(x: 0.6333504779f, y: 0.1926167129f, z: -0.7495104896f),
            new Float3(x: -0.06788241606f, y: 0.3998305789f, z: 0.9140719259f),
            new Float3(x: -0.5538628599f, y: -0.4729896695f, z: -0.6852128902f),
            new Float3(x: -0.7261455366f, y: -0.5911990757f, z: 0.3509933228f),
            new Float3(x: -0.9229274737f, y: -0.1782808786f, z: 0.3412049336f),
            new Float3(x: -0.6968815002f, y: 0.6511274338f, z: 0.3006480328f),
            new Float3(x: 0.9608044783f, y: -0.2098363234f, z: -0.1811724921f),
            new Float3(x: 0.06817146062f, y: -0.9743405129f, z: 0.2145069156f),
            new Float3(x: -0.3577285196f, y: -0.6697087264f, z: -0.6507845481f),
            new Float3(x: -0.1868621131f, y: 0.7648617052f, z: -0.6164974636f),
            new Float3(x: -0.6541697588f, y: 0.3967914832f, z: 0.6439087246f),
            new Float3(x: 0.6993340405f, y: -0.6164538506f, z: 0.3618239211f),
            new Float3(x: -0.1546665739f, y: 0.6291283928f, z: 0.7617583057f),
            new Float3(x: -0.6841612949f, y: -0.2580482182f, z: -0.6821542638f),
            new Float3(x: 0.5383980957f, y: 0.4258654885f, z: 0.7271630328f),
            new Float3(x: -0.5026987823f, y: -0.7939832935f, z: -0.3418836993f),
            new Float3(x: 0.3202971715f, y: 0.2834415347f, z: 0.9039195862f),
            new Float3(x: 0.8683227101f, y: -0.0003762656404f, z: -0.4959995258f),
            new Float3(x: 0.791120031f, y: -0.08511045745f, z: 0.6057105799f),
            new Float3(x: -0.04011016052f, y: -0.4397248749f, z: 0.8972364289f),
            new Float3(x: 0.9145119872f, y: 0.3579346169f, z: -0.1885487608f),
            new Float3(x: -0.9612039066f, y: -0.2756484276f, z: 0.01024666929f),
            new Float3(x: 0.6510361721f, y: -0.2877799159f, z: -0.7023778346f),
            new Float3(x: -0.2041786351f, y: 0.7365237271f, z: 0.644859585f),
            new Float3(x: -0.7718263711f, y: 0.3790626912f, z: 0.5104855816f),
            new Float3(x: -0.3060082741f, y: -0.7692987727f, z: 0.5608371729f),
            new Float3(x: 0.454007341f, y: -0.5024843065f, z: 0.7357899537f),
            new Float3(x: 0.4816795475f, y: 0.6021208291f, z: -0.6367380315f),
            new Float3(x: 0.6961980369f, y: -0.3222197429f, z: 0.641469197f),
            new Float3(x: -0.6532160499f, y: -0.6781148932f, z: 0.3368515753f),
            new Float3(x: 0.5089301236f, y: -0.6154662304f, z: -0.6018234363f),
            new Float3(x: -0.1635919754f, y: -0.9133604627f, z: -0.372840892f),
            new Float3(x: 0.52408019f, y: -0.8437664109f, z: 0.1157505864f),
            new Float3(x: 0.5902587356f, y: 0.4983817807f, z: -0.6349883666f),
            new Float3(x: 0.5863227872f, y: 0.494764745f, z: 0.6414307729f),
            new Float3(x: 0.6779335087f, y: 0.2341345225f, z: 0.6968408593f),
            new Float3(x: 0.7177054546f, y: -0.6858979348f, z: 0.120178631f),
            new Float3(x: -0.5328819713f, y: -0.5205125012f, z: 0.6671608058f),
            new Float3(x: -0.8654874251f, y: -0.0700727088f, z: -0.4960053754f),
            new Float3(x: -0.2861810166f, y: 0.7952089234f, z: 0.5345495242f),
            new Float3(x: -0.04849529634f, y: 0.9810836427f, z: -0.1874115585f),
            new Float3(x: -0.6358521667f, y: 0.6058348682f, z: 0.4781800233f),
            new Float3(x: 0.6254794696f, y: -0.2861619734f, z: 0.7258696564f),
            new Float3(x: -0.2585259868f, y: 0.5061949264f, z: -0.8227581726f),
            new Float3(x: 0.02136306781f, y: 0.5064016808f, z: -0.8620330371f),
            new Float3(x: 0.200111773f, y: 0.8599263484f, z: 0.4695550591f),
            new Float3(x: 0.4743561372f, y: 0.6014985084f, z: -0.6427953014f),
            new Float3(x: 0.6622993731f, y: -0.5202474575f, z: -0.5391679918f),
            new Float3(x: 0.08084972818f, y: -0.6532720452f, z: 0.7527940996f),
            new Float3(x: -0.6893687501f, y: 0.0592860349f, z: 0.7219805347f),
            new Float3(x: -0.1121887082f, y: -0.9673185067f, z: 0.2273952515f),
            new Float3(x: 0.7344116094f, y: 0.5979668656f, z: -0.3210532909f),
            new Float3(x: 0.5789393465f, y: -0.2488849713f, z: 0.7764570201f),
            new Float3(x: 0.6988182827f, y: 0.3557169806f, z: -0.6205791146f),
            new Float3(x: -0.8636845529f, y: -0.2748771249f, z: -0.4224826141f),
            new Float3(x: -0.4247027957f, y: -0.4640880967f, z: 0.777335046f),
            new Float3(x: 0.5257722489f, y: -0.8427017621f, z: 0.1158329937f),
            new Float3(x: 0.9343830603f, y: 0.316302472f, z: -0.1639543925f),
            new Float3(x: -0.1016836419f, y: -0.8057303073f, z: -0.5834887393f),
            new Float3(x: -0.6529238969f, y: 0.50602126f, z: -0.5635892736f),
            new Float3(x: -0.2465286165f, y: -0.9668205684f, z: -0.06694497494f),
            new Float3(x: -0.9776897119f, y: -0.2099250524f, z: -0.007368825344f),
            new Float3(x: 0.7736893337f, y: 0.5734244712f, z: 0.2694238123f),
            new Float3(x: -0.6095087895f, y: 0.4995678998f, z: 0.6155736747f),
            new Float3(x: 0.5794535482f, y: 0.7434546771f, z: 0.3339292269f),
            new Float3(x: -0.8226211154f, y: 0.08142581855f, z: 0.5627293636f),
            new Float3(x: -0.510385483f, y: 0.4703667658f, z: 0.7199039967f),
            new Float3(x: -0.5764971849f, y: -0.07231656274f, z: -0.8138926898f),
            new Float3(x: 0.7250628871f, y: 0.3949971505f, z: -0.5641463116f),
            new Float3(x: -0.1525424005f, y: 0.4860840828f, z: -0.8604958341f),
            new Float3(x: -0.5550976208f, y: -0.4957820792f, z: 0.667882296f),
            new Float3(x: -0.1883614327f, y: 0.9145869398f, z: 0.357841725f),
            new Float3(x: 0.7625556724f, y: -0.5414408243f, z: -0.3540489801f),
            new Float3(x: -0.5870231946f, y: -0.3226498013f, z: -0.7424963803f),
            new Float3(x: 0.3051124198f, y: 0.2262544068f, z: -0.9250488391f),
            new Float3(x: 0.6379576059f, y: 0.577242424f, z: -0.5097070502f),
            new Float3(x: -0.5966775796f, y: 0.1454852398f, z: -0.7891830656f),
            new Float3(x: -0.658330573f, y: 0.6555487542f, z: -0.3699414651f),
            new Float3(x: 0.7434892426f, y: 0.2351084581f, z: 0.6260573129f),
            new Float3(x: 0.5562114096f, y: 0.8264360377f, z: -0.0873632843f),
            new Float3(x: -0.3028940016f, y: -0.8251527185f, z: 0.4768419182f),
            new Float3(x: 0.1129343818f, y: -0.985888439f, z: -0.1235710781f),
            new Float3(x: 0.5937652891f, y: -0.5896813806f, z: 0.5474656618f),
            new Float3(x: 0.6757964092f, y: -0.5835758614f, z: -0.4502648413f),
            new Float3(x: 0.7242302609f, y: -0.1152719764f, z: 0.6798550586f),
            new Float3(x: -0.9511914166f, y: 0.0753623979f, z: -0.2992580792f),
            new Float3(x: 0.2539470961f, y: -0.1886339355f, z: 0.9486454084f),
            new Float3(x: 0.571433621f, y: -0.1679450851f, z: -0.8032795685f),
            new Float3(x: -0.06778234979f, y: 0.3978269256f, z: 0.9149531629f),
            new Float3(x: 0.6074972649f, y: 0.733060024f, z: -0.3058922593f),
            new Float3(x: -0.5435478392f, y: 0.1675822484f, z: 0.8224791405f),
            new Float3(x: -0.5876678086f, y: -0.3380045064f, z: -0.7351186982f),
            new Float3(x: -0.7967562402f, y: 0.04097822706f, z: -0.6029098428f),
            new Float3(x: -0.1996350917f, y: 0.8706294745f, z: 0.4496111079f),
            new Float3(x: -0.02787660336f, y: -0.9106232682f, z: -0.4122962022f),
            new Float3(x: -0.7797625996f, y: -0.6257634692f, z: 0.01975775581f),
            new Float3(x: -0.5211232846f, y: 0.7401644346f, z: -0.4249554471f),
            new Float3(x: 0.8575424857f, y: 0.4053272873f, z: -0.3167501783f),
            new Float3(x: 0.1045223322f, y: 0.8390195772f, z: -0.5339674439f),
            new Float3(x: 0.3501822831f, y: 0.9242524096f, z: -0.1520850155f),
            new Float3(x: 0.1987849858f, y: 0.07647613266f, z: 0.9770547224f),
            new Float3(x: 0.7845996363f, y: 0.6066256811f, z: -0.1280964233f),
            new Float3(x: 0.09006737436f, y: -0.9750989929f, z: -0.2026569073f),
            new Float3(x: -0.8274343547f, y: -0.542299559f, z: 0.1458203587f),
            new Float3(x: -0.3485797732f, y: -0.415802277f, z: 0.840000362f),
            new Float3(x: -0.2471778936f, y: -0.7304819962f, z: -0.6366310879f),
            new Float3(x: -0.3700154943f, y: 0.8577948156f, z: 0.3567584454f),
            new Float3(x: 0.5913394901f, y: -0.548311967f, z: -0.5913303597f),
            new Float3(x: 0.1204873514f, y: -0.7626472379f, z: -0.6354935001f),
            new Float3(x: 0.616959265f, y: 0.03079647928f, z: 0.7863922953f),
            new Float3(x: 0.1258156836f, y: -0.6640829889f, z: -0.7369967419f),
            new Float3(x: -0.6477565124f, y: -0.1740147258f, z: -0.7417077429f),
            new Float3(x: 0.6217889313f, y: -0.7804430448f, z: -0.06547655076f),
            new Float3(x: 0.6589943422f, y: -0.6096987708f, z: 0.4404473475f),
            new Float3(x: -0.2689837504f, y: -0.6732403169f, z: -0.6887635427f),
            new Float3(x: -0.3849775103f, y: 0.5676542638f, z: 0.7277093879f),
            new Float3(x: 0.5754444408f, y: 0.8110471154f, z: -0.1051963504f),
            new Float3(x: 0.9141593684f, y: 0.3832947817f, z: 0.131900567f),
            new Float3(x: -0.107925319f, y: 0.9245493968f, z: 0.3654593525f),
            new Float3(x: 0.377977089f, y: 0.3043148782f, z: 0.8743716458f),
            new Float3(x: -0.2142885215f, y: -0.8259286236f, z: 0.5214617324f),
            new Float3(x: 0.5802544474f, y: 0.4148098596f, z: -0.7008834116f),
            new Float3(x: -0.1982660881f, y: 0.8567161266f, z: -0.4761596756f),
            new Float3(x: -0.03381553704f, y: 0.3773180787f, z: -0.9254661404f),
            new Float3(x: -0.6867922841f, y: -0.6656597827f, z: 0.2919133642f),
            new Float3(x: 0.7731742607f, y: -0.2875793547f, z: -0.5652430251f),
            new Float3(x: -0.09655941928f, y: 0.9193708367f, z: -0.3813575004f),
            new Float3(x: 0.2715702457f, y: -0.9577909544f, z: -0.09426605581f),
            new Float3(x: 0.2451015704f, y: -0.6917998565f, z: -0.6792188003f),
            new Float3(x: 0.977700782f, y: -0.1753855374f, z: 0.1155036542f),
            new Float3(x: -0.5224739938f, y: 0.8521606816f, z: 0.02903615945f),
            new Float3(x: -0.7734880599f, y: -0.5261292347f, z: 0.3534179531f),
            new Float3(x: -0.7134492443f, y: -0.269547243f, z: 0.6467878011f),
            new Float3(x: 0.1644037271f, y: 0.5105846203f, z: -0.8439637196f),
            new Float3(x: 0.6494635788f, y: 0.05585611296f, z: 0.7583384168f),
            new Float3(x: -0.4711970882f, y: 0.5017280509f, z: -0.7254255765f),
            new Float3(x: -0.6335764307f, y: -0.2381686273f, z: -0.7361091029f),
            new Float3(x: -0.9021533097f, y: -0.270947803f, z: -0.3357181763f),
            new Float3(x: -0.3793711033f, y: 0.872258117f, z: 0.3086152025f),
            new Float3(x: -0.6855598966f, y: -0.3250143309f, z: 0.6514394162f),
            new Float3(x: 0.2900942212f, y: -0.7799057743f, z: -0.5546100667f),
            new Float3(x: -0.2098319339f, y: 0.85037073f, z: 0.4825351604f),
            new Float3(x: -0.4592603758f, y: 0.6598504336f, z: -0.5947077538f),
            new Float3(x: 0.8715945488f, y: 0.09616365406f, z: -0.4807031248f),
            new Float3(x: -0.6776666319f, y: 0.7118504878f, z: -0.1844907016f),
            new Float3(x: 0.7044377633f, y: 0.312427597f, z: 0.637304036f),
            new Float3(x: -0.7052318886f, y: -0.2401093292f, z: -0.6670798253f),
            new Float3(x: 0.081921007f, y: -0.7207336136f, z: -0.6883545647f),
            new Float3(x: -0.6993680906f, y: -0.5875763221f, z: -0.4069869034f),
            new Float3(x: -0.1281454481f, y: 0.6419895885f, z: 0.7559286424f),
            new Float3(x: -0.6337388239f, y: -0.6785471501f, z: -0.3714146849f),
            new Float3(x: 0.5565051903f, y: -0.2168887573f, z: -0.8020356851f),
            new Float3(x: -0.5791554484f, y: 0.7244372011f, z: -0.3738578718f),
            new Float3(x: 0.1175779076f, y: -0.7096451073f, z: 0.6946792478f),
            new Float3(x: -0.6134619607f, y: 0.1323631078f, z: 0.7785527795f),
            new Float3(x: 0.6984635305f, y: -0.02980516237f, z: -0.715024719f),
            new Float3(x: 0.8318082963f, y: -0.3930171956f, z: 0.3919597455f),
            new Float3(x: 0.1469576422f, y: 0.05541651717f, z: -0.9875892167f),
            new Float3(x: 0.708868575f, y: -0.2690503865f, z: 0.6520101478f),
            new Float3(x: 0.2726053183f, y: 0.67369766f, z: -0.68688995f),
            new Float3(x: -0.6591295371f, y: 0.3035458599f, z: -0.6880466294f),
            new Float3(x: 0.4815131379f, y: -0.7528270071f, z: 0.4487723203f),
            new Float3(x: 0.9430009463f, y: 0.1675647412f, z: -0.2875261255f),
            new Float3(x: 0.434802957f, y: 0.7695304522f, z: -0.4677277752f),
            new Float3(x: 0.3931996188f, y: 0.594473625f, z: 0.7014236729f),
            new Float3(x: 0.7254336655f, y: -0.603925654f, z: 0.3301814672f),
            new Float3(x: 0.7590235227f, y: -0.6506083235f, z: 0.02433313207f),
            new Float3(x: -0.8552768592f, y: -0.3430042733f, z: 0.3883935666f),
            new Float3(x: -0.6139746835f, y: 0.6981725247f, z: 0.3682257648f),
            new Float3(x: -0.7465905486f, y: -0.5752009504f, z: 0.3342849376f),
            new Float3(x: 0.5730065677f, y: 0.810555537f, z: -0.1210916791f),
            new Float3(x: -0.9225877367f, y: -0.3475211012f, z: -0.167514036f),
            new Float3(x: -0.7105816789f, y: -0.4719692027f, z: -0.5218416899f),
            new Float3(x: -0.08564609717f, y: 0.3583001386f, z: 0.929669703f),
            new Float3(x: -0.8279697606f, y: -0.2043157126f, z: 0.5222271202f),
            new Float3(x: 0.427944023f, y: 0.278165994f, z: 0.8599346446f),
            new Float3(x: 0.5399079671f, y: -0.7857120652f, z: -0.3019204161f),
            new Float3(x: 0.5678404253f, y: -0.5495413974f, z: -0.6128307303f),
            new Float3(x: -0.9896071041f, y: 0.1365639107f, z: -0.04503418428f),
            new Float3(x: -0.6154342638f, y: -0.6440875597f, z: 0.4543037336f),
            new Float3(x: 0.1074204368f, y: -0.7946340692f, z: 0.5975094525f),
            new Float3(x: -0.3595449969f, y: -0.8885529948f, z: 0.28495784f),
            new Float3(x: -0.2180405296f, y: 0.1529888965f, z: 0.9638738118f),
            new Float3(x: -0.7277432317f, y: -0.6164050508f, z: -0.3007234646f),
            new Float3(x: 0.7249729114f, y: -0.00669719484f, z: 0.6887448187f),
            new Float3(x: -0.5553659455f, y: -0.5336586252f, z: 0.6377908264f),
            new Float3(x: 0.5137558015f, y: 0.7976208196f, z: -0.3160000073f),
            new Float3(x: -0.3794024848f, y: 0.9245608561f, z: -0.03522751494f),
            new Float3(x: 0.8229248658f, y: 0.2745365933f, z: -0.4974176556f),
            new Float3(x: -0.5404114394f, y: 0.6091141441f, z: 0.5804613989f),
            new Float3(x: 0.8036581901f, y: -0.2703029469f, z: 0.5301601931f),
            new Float3(x: 0.6044318879f, y: 0.6832968393f, z: 0.4095943388f),
            new Float3(x: 0.06389988817f, y: 0.9658208605f, z: -0.2512108074f),
            new Float3(x: 0.1087113286f, y: 0.7402471173f, z: -0.6634877936f),
            new Float3(x: -0.713427712f, y: -0.6926784018f, z: 0.1059128479f),
            new Float3(x: 0.6458897819f, y: -0.5724548511f, z: -0.5050958653f),
            new Float3(x: -0.6553931414f, y: 0.7381471625f, z: 0.159995615f),
            new Float3(x: 0.3910961323f, y: 0.9188871375f, z: -0.05186755998f),
            new Float3(x: -0.4879022471f, y: -0.5904376907f, z: 0.6429111375f),
            new Float3(x: 0.6014790094f, y: 0.7707441366f, z: -0.2101820095f),
            new Float3(x: -0.5677173047f, y: 0.7511360995f, z: 0.3368851762f),
            new Float3(x: 0.7858573506f, y: 0.226674665f, z: 0.5753666838f),
            new Float3(x: -0.4520345543f, y: -0.604222686f, z: -0.6561857263f),
            new Float3(x: 0.002272116345f, y: 0.4132844051f, z: -0.9105991643f),
            new Float3(x: -0.5815751419f, y: -0.5162925989f, z: 0.6286591339f),
            new Float3(x: -0.03703704785f, y: 0.8273785755f, z: 0.5604221175f),
            new Float3(x: -0.5119692504f, y: 0.7953543429f, z: -0.3244980058f),
            new Float3(x: -0.2682417366f, y: -0.9572290247f, z: -0.1084387619f),
            new Float3(x: -0.2322482736f, y: -0.9679131102f, z: -0.09594243324f),
            new Float3(x: 0.3554328906f, y: -0.8881505545f, z: 0.2913006227f),
            new Float3(x: 0.7346520519f, y: -0.4371373164f, z: 0.5188422971f),
            new Float3(x: 0.9985120116f, y: 0.04659011161f, z: -0.02833944577f),
            new Float3(x: -0.3727687496f, y: -0.9082481361f, z: 0.1900757285f),
            new Float3(x: 0.91737377f, y: -0.3483642108f, z: 0.1925298489f),
            new Float3(x: 0.2714911074f, y: 0.4147529736f, z: -0.8684886582f),
            new Float3(x: 0.5131763485f, y: -0.7116334161f, z: 0.4798207128f),
            new Float3(x: -0.8737353606f, y: 0.18886992f, z: -0.4482350644f),
            new Float3(x: 0.8460043821f, y: -0.3725217914f, z: 0.3814499973f),
            new Float3(x: 0.8978727456f, y: -0.1780209141f, z: -0.4026575304f),
            new Float3(x: 0.2178065647f, y: -0.9698322841f, z: -0.1094789531f),
            new Float3(x: -0.1518031304f, y: -0.7788918132f, z: -0.6085091231f),
            new Float3(x: -0.2600384876f, y: -0.4755398075f, z: -0.8403819825f),
            new Float3(x: 0.572313509f, y: -0.7474340931f, z: -0.3373418503f),
            new Float3(x: -0.7174141009f, y: 0.1699017182f, z: -0.6756111411f),
            new Float3(x: -0.684180784f, y: 0.02145707593f, z: -0.7289967412f),
            new Float3(x: -0.2007447902f, y: 0.06555605789f, z: -0.9774476623f),
            new Float3(x: -0.1148803697f, y: -0.8044887315f, z: 0.5827524187f),
            new Float3(x: -0.7870349638f, y: 0.03447489231f, z: 0.6159443543f),
            new Float3(x: -0.2015596421f, y: 0.6859872284f, z: 0.6991389226f),
            new Float3(x: -0.08581082512f, y: -0.10920836f, z: -0.9903080513f),
            new Float3(x: 0.5532693395f, y: 0.7325250401f, z: -0.396610771f),
            new Float3(x: -0.1842489331f, y: -0.9777375055f, z: -0.1004076743f),
            new Float3(x: 0.0775473789f, y: -0.9111505856f, z: 0.4047110257f),
            new Float3(x: 0.1399838409f, y: 0.7601631212f, z: -0.6344734459f),
            new Float3(x: 0.4484419361f, y: -0.845289248f, z: 0.2904925424f)
        };

        private static readonly byte[] SIMPLEX_4D =
        {
            0, 1, 2, 3, 0, 1, 3, 2, 0, 0, 0, 0, 0, 2, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 0,
            0, 2, 1, 3, 0, 0, 0, 0, 0, 3, 1, 2, 0, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 2, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 2, 0, 3, 0, 0, 0, 0, 1, 3, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 0, 1, 2, 3, 1, 0,
            1, 0, 2, 3, 1, 0, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 3, 1, 0, 0, 0, 0, 2, 1, 3, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2, 0, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 1, 2, 3, 0, 2, 1, 0, 0, 0, 0, 3, 1, 2, 0,
            2, 1, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 1, 0, 2, 0, 0, 0, 0, 3, 2, 0, 1, 3, 2, 1, 0
        };

        private CellularDistanceFunction m_cellularDistanceFunction = CellularDistanceFunction.Euclidean;
        private int m_cellularDistanceIndex0 = 0;
        private int m_cellularDistanceIndex1 = 1;
        private float m_cellularJitter = 0.45f;
        private FastNoise m_cellularNoiseLookup = null!;
        private CellularReturnType m_cellularReturnType = CellularReturnType.CellValue;

        private FN_DECIMAL m_fractalBounding;
        private FractalType m_fractalType = FractalType.FBM;
        private FN_DECIMAL m_frequency = (FN_DECIMAL) 0.01;
        private FN_DECIMAL m_gain = (FN_DECIMAL) 0.5;

        private FN_DECIMAL m_gradientPerturbAmp = (FN_DECIMAL) 1.0;
        private Interp m_interp = Interp.Quintic;
        private FN_DECIMAL m_lacunarity = (FN_DECIMAL) 2.0;
        private NoiseType m_noiseType = NoiseType.Simplex;

        private int m_octaves = 3;

        private int m_seed = 1337;

        public FastNoise(int seed = 1337)
        {
            m_seed = seed;
            CalculateFractalBounding();
        }

        // Returns a 0 float/double
        public static FN_DECIMAL GetDecimalType()
        {
            return 0;
        }

        // Returns the seed used by this object
        public int GetSeed()
        {
            return m_seed;
        }

        // Sets seed used for all noise types
        // Default: 1337
        public void SetSeed(int seed)
        {
            m_seed = seed;
        }

        // Sets frequency for all noise types
        // Default: 0.01
        public void SetFrequency(FN_DECIMAL frequency)
        {
            m_frequency = frequency;
        }

        // Changes the interpolation method used to smooth between noise values
        // Possible interpolation methods (lowest to highest quality) :
        // - Linear
        // - Hermite
        // - Quintic
        // Used in Value, Gradient Noise and Position Perturbing
        // Default: Quintic
        public void SetInterp(Interp interp)
        {
            m_interp = interp;
        }

        // Sets noise return type of GetNoise(...)
        // Default: Simplex
        public void SetNoiseType(NoiseType noiseType)
        {
            m_noiseType = noiseType;
        }

        // Sets octave count for all fractal noise types
        // Default: 3
        public void SetFractalOctaves(int octaves)
        {
            m_octaves = octaves;
            CalculateFractalBounding();
        }

        // Sets octave lacunarity for all fractal noise types
        // Default: 2.0
        public void SetFractalLacunarity(FN_DECIMAL lacunarity)
        {
            m_lacunarity = lacunarity;
        }

        // Sets octave gain for all fractal noise types
        // Default: 0.5
        public void SetFractalGain(FN_DECIMAL gain)
        {
            m_gain = gain;
            CalculateFractalBounding();
        }

        // Sets method for combining octaves in all fractal noise types
        // Default: FBM
        public void SetFractalType(FractalType fractalType)
        {
            m_fractalType = fractalType;
        }

        // Sets return type from cellular noise calculations
        // Note: NoiseLookup requires another FastNoise object be set with SetCellularNoiseLookup() to function
        // Default: CellValue
        public void SetCellularDistanceFunction(CellularDistanceFunction cellularDistanceFunction)
        {
            m_cellularDistanceFunction = cellularDistanceFunction;
        }

        // Sets distance function used in cellular noise calculations
        // Default: Euclidean
        public void SetCellularReturnType(CellularReturnType cellularReturnType)
        {
            m_cellularReturnType = cellularReturnType;
        }

        // Sets the 2 distance indicies used for distance2 return types
        // Default: 0, 1
        // Note: index0 should be lower than index1
        // Both indicies must be >= 0, index1 must be < 4
        public void SetCellularDistance2Indicies(int cellularDistanceIndex0, int cellularDistanceIndex1)
        {
            m_cellularDistanceIndex0 = Math.Min(cellularDistanceIndex0, cellularDistanceIndex1);
            m_cellularDistanceIndex1 = Math.Max(cellularDistanceIndex0, cellularDistanceIndex1);

            m_cellularDistanceIndex0 = Math.Min(Math.Max(m_cellularDistanceIndex0, val2: 0), FN_CELLULAR_INDEX_MAX);
            m_cellularDistanceIndex1 = Math.Min(Math.Max(m_cellularDistanceIndex1, val2: 0), FN_CELLULAR_INDEX_MAX);
        }

        // Sets the maximum distance a cellular point can move from it's grid position
        // Setting this high will make artifacts more common
        // Default: 0.45
        public void SetCellularJitter(float cellularJitter)
        {
            m_cellularJitter = cellularJitter;
        }

        // Noise used to calculate a cell value if cellular return type is NoiseLookup
        // The lookup value is acquired through GetNoise() so ensure you SetNoiseType() on the noise lookup, value, gradient or simplex is recommended
        public void SetCellularNoiseLookup(FastNoise noise)
        {
            m_cellularNoiseLookup = noise;
        }

        // Sets the maximum perturb distance from original location when using GradientPerturb{Fractal}(...)
        // Default: 1.0
        public void SetGradientPerturbAmp(FN_DECIMAL gradientPerturbAmp)
        {
            m_gradientPerturbAmp = gradientPerturbAmp;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static int FastFloor(FN_DECIMAL f)
        {
            return (f >= 0 ? (int) f : (int) f - 1);
        }

        [MethodImplAttribute(FN_INLINE)]
        private static int FastRound(FN_DECIMAL f)
        {
            return (f >= 0) ? (int) (f + (FN_DECIMAL) 0.5) : (int) (f - (FN_DECIMAL) 0.5);
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL Lerp(FN_DECIMAL a, FN_DECIMAL b, FN_DECIMAL t)
        {
            return a + t * (b - a);
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL InterpHermiteFunc(FN_DECIMAL t)
        {
            return t * t * (3 - 2 * t);
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL InterpQuinticFunc(FN_DECIMAL t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL CubicLerp(FN_DECIMAL a, FN_DECIMAL b, FN_DECIMAL c, FN_DECIMAL d, FN_DECIMAL t)
        {
            FN_DECIMAL p = (d - c) - (a - b);

            return t * t * t * p + t * t * ((a - b) - p) + t * (c - a) + b;
        }

        private void CalculateFractalBounding()
        {
            FN_DECIMAL amp = m_gain;
            FN_DECIMAL ampFractal = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                ampFractal += amp;
                amp *= m_gain;
            }

            m_fractalBounding = 1 / ampFractal;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static int Hash2D(int seed, int x, int y)
        {
            int hash = seed;
            hash ^= X_PRIME * x;
            hash ^= Y_PRIME * y;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static int Hash3D(int seed, int x, int y, int z)
        {
            int hash = seed;
            hash ^= X_PRIME * x;
            hash ^= Y_PRIME * y;
            hash ^= Z_PRIME * z;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static int Hash4D(int seed, int x, int y, int z, int w)
        {
            int hash = seed;
            hash ^= X_PRIME * x;
            hash ^= Y_PRIME * y;
            hash ^= Z_PRIME * z;
            hash ^= W_PRIME * w;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL ValCoord2D(int seed, int x, int y)
        {
            int n = seed;
            n ^= X_PRIME * x;
            n ^= Y_PRIME * y;

            return (n * n * n * 60493) / (FN_DECIMAL) 2147483648.0;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL ValCoord3D(int seed, int x, int y, int z)
        {
            int n = seed;
            n ^= X_PRIME * x;
            n ^= Y_PRIME * y;
            n ^= Z_PRIME * z;

            return (n * n * n * 60493) / (FN_DECIMAL) 2147483648.0;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL ValCoord4D(int seed, int x, int y, int z, int w)
        {
            int n = seed;
            n ^= X_PRIME * x;
            n ^= Y_PRIME * y;
            n ^= Z_PRIME * z;
            n ^= W_PRIME * w;

            return (n * n * n * 60493) / (FN_DECIMAL) 2147483648.0;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL GradCoord2D(int seed, int x, int y, FN_DECIMAL xd, FN_DECIMAL yd)
        {
            int hash = seed;
            hash ^= X_PRIME * x;
            hash ^= Y_PRIME * y;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            Float2 g = GRAD_2D[hash & 7];

            return xd * g.x + yd * g.y;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL GradCoord3D(int seed, int x, int y, int z, FN_DECIMAL xd, FN_DECIMAL yd,
            FN_DECIMAL zd)
        {
            int hash = seed;
            hash ^= X_PRIME * x;
            hash ^= Y_PRIME * y;
            hash ^= Z_PRIME * z;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            Float3 g = GRAD_3D[hash & 15];

            return xd * g.x + yd * g.y + zd * g.z;
        }

        [MethodImplAttribute(FN_INLINE)]
        private static FN_DECIMAL GradCoord4D(int seed, int x, int y, int z, int w, FN_DECIMAL xd, FN_DECIMAL yd,
            FN_DECIMAL zd, FN_DECIMAL wd)
        {
            int hash = seed;
            hash ^= X_PRIME * x;
            hash ^= Y_PRIME * y;
            hash ^= Z_PRIME * z;
            hash ^= W_PRIME * w;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            hash &= 31;
            FN_DECIMAL a = yd, b = zd, c = wd; // X,Y,Z

            switch (hash >> 3)
            {
                // OR, DEPENDING ON HIGH ORDER 2 BITS:
                case 1:
                    a = wd;
                    b = xd;
                    c = yd;

                    break; // W,X,Y
                case 2:
                    a = zd;
                    b = wd;
                    c = xd;

                    break; // Z,W,X
                case 3:
                    a = yd;
                    b = zd;
                    c = wd;

                    break; // Y,Z,W
            }

            return ((hash & 4) == 0 ? -a : a) + ((hash & 2) == 0 ? -b : b) + ((hash & 1) == 0 ? -c : c);
        }

        public FN_DECIMAL GetNoise(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            x *= m_frequency;
            y *= m_frequency;
            z *= m_frequency;

            switch (m_noiseType)
            {
                case NoiseType.Value:
                    return SingleValue(m_seed, x, y, z);

                case NoiseType.ValueFractal:
                    switch (m_fractalType)
                    {
                        case FractalType.FBM:
                            return SingleValueFractalFBM(x, y, z);

                        case FractalType.Billow:
                            return SingleValueFractalBillow(x, y, z);

                        case FractalType.RigidMulti:
                            return SingleValueFractalRigidMulti(x, y, z);

                        default:
                            return 0;
                    }
                case NoiseType.Perlin:
                    return SinglePerlin(m_seed, x, y, z);

                case NoiseType.PerlinFractal:
                    switch (m_fractalType)
                    {
                        case FractalType.FBM:
                            return SinglePerlinFractalFBM(x, y, z);

                        case FractalType.Billow:
                            return SinglePerlinFractalBillow(x, y, z);

                        case FractalType.RigidMulti:
                            return SinglePerlinFractalRigidMulti(x, y, z);

                        default:
                            return 0;
                    }
                case NoiseType.Simplex:
                    return SingleSimplex(m_seed, x, y, z);

                case NoiseType.SimplexFractal:
                    switch (m_fractalType)
                    {
                        case FractalType.FBM:
                            return SingleSimplexFractalFBM(x, y, z);

                        case FractalType.Billow:
                            return SingleSimplexFractalBillow(x, y, z);

                        case FractalType.RigidMulti:
                            return SingleSimplexFractalRigidMulti(x, y, z);

                        default:
                            return 0;
                    }
                case NoiseType.Cellular:
                    switch (m_cellularReturnType)
                    {
                        case CellularReturnType.CellValue:
                        case CellularReturnType.NoiseLookup:
                        case CellularReturnType.Distance:
                            return SingleCellular(x, y, z);

                        default:
                            return SingleCellular2Edge(x, y, z);
                    }
                case NoiseType.WhiteNoise:
                    return GetWhiteNoise(x, y, z);

                case NoiseType.Cubic:
                    return SingleCubic(m_seed, x, y, z);

                case NoiseType.CubicFractal:
                    switch (m_fractalType)
                    {
                        case FractalType.FBM:
                            return SingleCubicFractalFBM(x, y, z);

                        case FractalType.Billow:
                            return SingleCubicFractalBillow(x, y, z);

                        case FractalType.RigidMulti:
                            return SingleCubicFractalRigidMulti(x, y, z);

                        default:
                            return 0;
                    }
                default:
                    return 0;
            }
        }

        public FN_DECIMAL GetNoise(FN_DECIMAL x, FN_DECIMAL y)
        {
            x *= m_frequency;
            y *= m_frequency;

            switch (m_noiseType)
            {
                case NoiseType.Value:
                    return SingleValue(m_seed, x, y);

                case NoiseType.ValueFractal:
                    switch (m_fractalType)
                    {
                        case FractalType.FBM:
                            return SingleValueFractalFBM(x, y);

                        case FractalType.Billow:
                            return SingleValueFractalBillow(x, y);

                        case FractalType.RigidMulti:
                            return SingleValueFractalRigidMulti(x, y);

                        default:
                            return 0;
                    }
                case NoiseType.Perlin:
                    return SinglePerlin(m_seed, x, y);

                case NoiseType.PerlinFractal:
                    switch (m_fractalType)
                    {
                        case FractalType.FBM:
                            return SinglePerlinFractalFBM(x, y);

                        case FractalType.Billow:
                            return SinglePerlinFractalBillow(x, y);

                        case FractalType.RigidMulti:
                            return SinglePerlinFractalRigidMulti(x, y);

                        default:
                            return 0;
                    }
                case NoiseType.Simplex:
                    return SingleSimplex(m_seed, x, y);

                case NoiseType.SimplexFractal:
                    switch (m_fractalType)
                    {
                        case FractalType.FBM:
                            return SingleSimplexFractalFBM(x, y);

                        case FractalType.Billow:
                            return SingleSimplexFractalBillow(x, y);

                        case FractalType.RigidMulti:
                            return SingleSimplexFractalRigidMulti(x, y);

                        default:
                            return 0;
                    }
                case NoiseType.Cellular:
                    switch (m_cellularReturnType)
                    {
                        case CellularReturnType.CellValue:
                        case CellularReturnType.NoiseLookup:
                        case CellularReturnType.Distance:
                            return SingleCellular(x, y);

                        default:
                            return SingleCellular2Edge(x, y);
                    }
                case NoiseType.WhiteNoise:
                    return GetWhiteNoise(x, y);

                case NoiseType.Cubic:
                    return SingleCubic(m_seed, x, y);

                case NoiseType.CubicFractal:
                    switch (m_fractalType)
                    {
                        case FractalType.FBM:
                            return SingleCubicFractalFBM(x, y);

                        case FractalType.Billow:
                            return SingleCubicFractalBillow(x, y);

                        case FractalType.RigidMulti:
                            return SingleCubicFractalRigidMulti(x, y);

                        default:
                            return 0;
                    }
                default:
                    return 0;
            }
        }

        // White Noise
        [MethodImplAttribute(FN_INLINE)]
        private static int FloatCast2Int(FN_DECIMAL f)
        {
            var i = BitConverter.DoubleToInt64Bits(f);

            return (int) (i ^ (i >> 32));
        }

        public FN_DECIMAL GetWhiteNoise(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z, FN_DECIMAL w)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);
            int zi = FloatCast2Int(z);
            int wi = FloatCast2Int(w);

            return ValCoord4D(m_seed, xi, yi, zi, wi);
        }

        public FN_DECIMAL GetWhiteNoise(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);
            int zi = FloatCast2Int(z);

            return ValCoord3D(m_seed, xi, yi, zi);
        }

        public FN_DECIMAL GetWhiteNoise(FN_DECIMAL x, FN_DECIMAL y)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);

            return ValCoord2D(m_seed, xi, yi);
        }

        public FN_DECIMAL GetWhiteNoiseInt(int x, int y, int z, int w)
        {
            return ValCoord4D(m_seed, x, y, z, w);
        }

        public FN_DECIMAL GetWhiteNoiseInt(int x, int y, int z)
        {
            return ValCoord3D(m_seed, x, y, z);
        }

        public FN_DECIMAL GetWhiteNoiseInt(int x, int y)
        {
            return ValCoord2D(m_seed, x, y);
        }

        // Value Noise
        public FN_DECIMAL GetValueFractal(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            x *= m_frequency;
            y *= m_frequency;
            z *= m_frequency;

            switch (m_fractalType)
            {
                case FractalType.FBM:
                    return SingleValueFractalFBM(x, y, z);

                case FractalType.Billow:
                    return SingleValueFractalBillow(x, y, z);

                case FractalType.RigidMulti:
                    return SingleValueFractalRigidMulti(x, y, z);

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SingleValueFractalFBM(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = SingleValue(seed, x, y, z);
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum += SingleValue(++seed, x, y, z) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleValueFractalBillow(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = Math.Abs(SingleValue(seed, x, y, z)) * 2 - 1;
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum += (Math.Abs(SingleValue(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleValueFractalRigidMulti(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = 1 - Math.Abs(SingleValue(seed, x, y, z));
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum -= (1 - Math.Abs(SingleValue(++seed, x, y, z))) * amp;
            }

            return sum;
        }

        public FN_DECIMAL GetValue(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            return SingleValue(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
        }

        private FN_DECIMAL SingleValue(int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int x0 = FastFloor(x);
            int y0 = FastFloor(y);
            int z0 = FastFloor(z);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;

            FN_DECIMAL xs, ys, zs;

            switch (m_interp)
            {
                default:
                case Interp.Linear:
                    xs = x - x0;
                    ys = y - y0;
                    zs = z - z0;

                    break;

                case Interp.Hermite:
                    xs = InterpHermiteFunc(x - x0);
                    ys = InterpHermiteFunc(y - y0);
                    zs = InterpHermiteFunc(z - z0);

                    break;

                case Interp.Quintic:
                    xs = InterpQuinticFunc(x - x0);
                    ys = InterpQuinticFunc(y - y0);
                    zs = InterpQuinticFunc(z - z0);

                    break;
            }

            FN_DECIMAL xf00 = Lerp(ValCoord3D(seed, x0, y0, z0), ValCoord3D(seed, x1, y0, z0), xs);
            FN_DECIMAL xf10 = Lerp(ValCoord3D(seed, x0, y1, z0), ValCoord3D(seed, x1, y1, z0), xs);
            FN_DECIMAL xf01 = Lerp(ValCoord3D(seed, x0, y0, z1), ValCoord3D(seed, x1, y0, z1), xs);
            FN_DECIMAL xf11 = Lerp(ValCoord3D(seed, x0, y1, z1), ValCoord3D(seed, x1, y1, z1), xs);

            FN_DECIMAL yf0 = Lerp(xf00, xf10, ys);
            FN_DECIMAL yf1 = Lerp(xf01, xf11, ys);

            return Lerp(yf0, yf1, zs);
        }

        public FN_DECIMAL GetValueFractal(FN_DECIMAL x, FN_DECIMAL y)
        {
            x *= m_frequency;
            y *= m_frequency;

            switch (m_fractalType)
            {
                case FractalType.FBM:
                    return SingleValueFractalFBM(x, y);

                case FractalType.Billow:
                    return SingleValueFractalBillow(x, y);

                case FractalType.RigidMulti:
                    return SingleValueFractalRigidMulti(x, y);

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SingleValueFractalFBM(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = SingleValue(seed, x, y);
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum += SingleValue(++seed, x, y) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleValueFractalBillow(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = Math.Abs(SingleValue(seed, x, y)) * 2 - 1;
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                amp *= m_gain;
                sum += (Math.Abs(SingleValue(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleValueFractalRigidMulti(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = 1 - Math.Abs(SingleValue(seed, x, y));
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum -= (1 - Math.Abs(SingleValue(++seed, x, y))) * amp;
            }

            return sum;
        }

        public FN_DECIMAL GetValue(FN_DECIMAL x, FN_DECIMAL y)
        {
            return SingleValue(m_seed, x * m_frequency, y * m_frequency);
        }

        private FN_DECIMAL SingleValue(int seed, FN_DECIMAL x, FN_DECIMAL y)
        {
            int x0 = FastFloor(x);
            int y0 = FastFloor(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            FN_DECIMAL xs, ys;

            switch (m_interp)
            {
                default:
                case Interp.Linear:
                    xs = x - x0;
                    ys = y - y0;

                    break;

                case Interp.Hermite:
                    xs = InterpHermiteFunc(x - x0);
                    ys = InterpHermiteFunc(y - y0);

                    break;

                case Interp.Quintic:
                    xs = InterpQuinticFunc(x - x0);
                    ys = InterpQuinticFunc(y - y0);

                    break;
            }

            FN_DECIMAL xf0 = Lerp(ValCoord2D(seed, x0, y0), ValCoord2D(seed, x1, y0), xs);
            FN_DECIMAL xf1 = Lerp(ValCoord2D(seed, x0, y1), ValCoord2D(seed, x1, y1), xs);

            return Lerp(xf0, xf1, ys);
        }

        // Gradient Noise
        public FN_DECIMAL GetPerlinFractal(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            x *= m_frequency;
            y *= m_frequency;
            z *= m_frequency;

            switch (m_fractalType)
            {
                case FractalType.FBM:
                    return SinglePerlinFractalFBM(x, y, z);

                case FractalType.Billow:
                    return SinglePerlinFractalBillow(x, y, z);

                case FractalType.RigidMulti:
                    return SinglePerlinFractalRigidMulti(x, y, z);

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SinglePerlinFractalFBM(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = SinglePerlin(seed, x, y, z);
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum += SinglePerlin(++seed, x, y, z) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SinglePerlinFractalBillow(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = Math.Abs(SinglePerlin(seed, x, y, z)) * 2 - 1;
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum += (Math.Abs(SinglePerlin(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SinglePerlinFractalRigidMulti(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = 1 - Math.Abs(SinglePerlin(seed, x, y, z));
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum -= (1 - Math.Abs(SinglePerlin(++seed, x, y, z))) * amp;
            }

            return sum;
        }

        public FN_DECIMAL GetPerlin(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            return SinglePerlin(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
        }

        private FN_DECIMAL SinglePerlin(int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int x0 = FastFloor(x);
            int y0 = FastFloor(y);
            int z0 = FastFloor(z);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;

            FN_DECIMAL xs, ys, zs;

            switch (m_interp)
            {
                default:
                case Interp.Linear:
                    xs = x - x0;
                    ys = y - y0;
                    zs = z - z0;

                    break;

                case Interp.Hermite:
                    xs = InterpHermiteFunc(x - x0);
                    ys = InterpHermiteFunc(y - y0);
                    zs = InterpHermiteFunc(z - z0);

                    break;

                case Interp.Quintic:
                    xs = InterpQuinticFunc(x - x0);
                    ys = InterpQuinticFunc(y - y0);
                    zs = InterpQuinticFunc(z - z0);

                    break;
            }

            FN_DECIMAL xd0 = x - x0;
            FN_DECIMAL yd0 = y - y0;
            FN_DECIMAL zd0 = z - z0;
            FN_DECIMAL xd1 = xd0 - 1;
            FN_DECIMAL yd1 = yd0 - 1;
            FN_DECIMAL zd1 = zd0 - 1;

            FN_DECIMAL xf00 = Lerp(
                GradCoord3D(seed, x0, y0, z0, xd0, yd0, zd0),
                GradCoord3D(seed, x1, y0, z0, xd1, yd0, zd0),
                xs);

            FN_DECIMAL xf10 = Lerp(
                GradCoord3D(seed, x0, y1, z0, xd0, yd1, zd0),
                GradCoord3D(seed, x1, y1, z0, xd1, yd1, zd0),
                xs);

            FN_DECIMAL xf01 = Lerp(
                GradCoord3D(seed, x0, y0, z1, xd0, yd0, zd1),
                GradCoord3D(seed, x1, y0, z1, xd1, yd0, zd1),
                xs);

            FN_DECIMAL xf11 = Lerp(
                GradCoord3D(seed, x0, y1, z1, xd0, yd1, zd1),
                GradCoord3D(seed, x1, y1, z1, xd1, yd1, zd1),
                xs);

            FN_DECIMAL yf0 = Lerp(xf00, xf10, ys);
            FN_DECIMAL yf1 = Lerp(xf01, xf11, ys);

            return Lerp(yf0, yf1, zs);
        }

        public FN_DECIMAL GetPerlinFractal(FN_DECIMAL x, FN_DECIMAL y)
        {
            x *= m_frequency;
            y *= m_frequency;

            switch (m_fractalType)
            {
                case FractalType.FBM:
                    return SinglePerlinFractalFBM(x, y);

                case FractalType.Billow:
                    return SinglePerlinFractalBillow(x, y);

                case FractalType.RigidMulti:
                    return SinglePerlinFractalRigidMulti(x, y);

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SinglePerlinFractalFBM(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = SinglePerlin(seed, x, y);
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum += SinglePerlin(++seed, x, y) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SinglePerlinFractalBillow(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = Math.Abs(SinglePerlin(seed, x, y)) * 2 - 1;
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum += (Math.Abs(SinglePerlin(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SinglePerlinFractalRigidMulti(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = 1 - Math.Abs(SinglePerlin(seed, x, y));
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum -= (1 - Math.Abs(SinglePerlin(++seed, x, y))) * amp;
            }

            return sum;
        }

        public FN_DECIMAL GetPerlin(FN_DECIMAL x, FN_DECIMAL y)
        {
            return SinglePerlin(m_seed, x * m_frequency, y * m_frequency);
        }

        private FN_DECIMAL SinglePerlin(int seed, FN_DECIMAL x, FN_DECIMAL y)
        {
            int x0 = FastFloor(x);
            int y0 = FastFloor(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            FN_DECIMAL xs, ys;

            switch (m_interp)
            {
                default:
                case Interp.Linear:
                    xs = x - x0;
                    ys = y - y0;

                    break;

                case Interp.Hermite:
                    xs = InterpHermiteFunc(x - x0);
                    ys = InterpHermiteFunc(y - y0);

                    break;

                case Interp.Quintic:
                    xs = InterpQuinticFunc(x - x0);
                    ys = InterpQuinticFunc(y - y0);

                    break;
            }

            FN_DECIMAL xd0 = x - x0;
            FN_DECIMAL yd0 = y - y0;
            FN_DECIMAL xd1 = xd0 - 1;
            FN_DECIMAL yd1 = yd0 - 1;

            FN_DECIMAL xf0 = Lerp(GradCoord2D(seed, x0, y0, xd0, yd0), GradCoord2D(seed, x1, y0, xd1, yd0), xs);
            FN_DECIMAL xf1 = Lerp(GradCoord2D(seed, x0, y1, xd0, yd1), GradCoord2D(seed, x1, y1, xd1, yd1), xs);

            return Lerp(xf0, xf1, ys);
        }

        // Simplex Noise
        public FN_DECIMAL GetSimplexFractal(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            x *= m_frequency;
            y *= m_frequency;
            z *= m_frequency;

            switch (m_fractalType)
            {
                case FractalType.FBM:
                    return SingleSimplexFractalFBM(x, y, z);

                case FractalType.Billow:
                    return SingleSimplexFractalBillow(x, y, z);

                case FractalType.RigidMulti:
                    return SingleSimplexFractalRigidMulti(x, y, z);

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SingleSimplexFractalFBM(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = SingleSimplex(seed, x, y, z);
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum += SingleSimplex(++seed, x, y, z) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleSimplexFractalBillow(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = Math.Abs(SingleSimplex(seed, x, y, z)) * 2 - 1;
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum += (Math.Abs(SingleSimplex(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleSimplexFractalRigidMulti(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = 1 - Math.Abs(SingleSimplex(seed, x, y, z));
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum -= (1 - Math.Abs(SingleSimplex(++seed, x, y, z))) * amp;
            }

            return sum;
        }

        public FN_DECIMAL GetSimplex(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            return SingleSimplex(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
        }

        private static FN_DECIMAL SingleSimplex(int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            FN_DECIMAL t = (x + y + z) * F3;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);
            int k = FastFloor(z + t);

            t = (i + j + k) * G3;
            FN_DECIMAL x0 = x - (i - t);
            FN_DECIMAL y0 = y - (j - t);
            FN_DECIMAL z0 = z - (k - t);

            int i1, j1, k1;
            int i2, j2, k2;

            if (x0 >= y0)
            {
                if (y0 >= z0)
                {
                    i1 = 1;
                    j1 = 0;
                    k1 = 0;
                    i2 = 1;
                    j2 = 1;
                    k2 = 0;
                }
                else if (x0 >= z0)
                {
                    i1 = 1;
                    j1 = 0;
                    k1 = 0;
                    i2 = 1;
                    j2 = 0;
                    k2 = 1;
                }
                else // x0 < z0
                {
                    i1 = 0;
                    j1 = 0;
                    k1 = 1;
                    i2 = 1;
                    j2 = 0;
                    k2 = 1;
                }
            }
            else // x0 < y0
            {
                if (y0 < z0)
                {
                    i1 = 0;
                    j1 = 0;
                    k1 = 1;
                    i2 = 0;
                    j2 = 1;
                    k2 = 1;
                }
                else if (x0 < z0)
                {
                    i1 = 0;
                    j1 = 1;
                    k1 = 0;
                    i2 = 0;
                    j2 = 1;
                    k2 = 1;
                }
                else // x0 >= z0
                {
                    i1 = 0;
                    j1 = 1;
                    k1 = 0;
                    i2 = 1;
                    j2 = 1;
                    k2 = 0;
                }
            }

            FN_DECIMAL x1 = x0 - i1 + G3;
            FN_DECIMAL y1 = y0 - j1 + G3;
            FN_DECIMAL z1 = z0 - k1 + G3;
            FN_DECIMAL x2 = x0 - i2 + F3;
            FN_DECIMAL y2 = y0 - j2 + F3;
            FN_DECIMAL z2 = z0 - k2 + F3;
            FN_DECIMAL x3 = x0 + G33;
            FN_DECIMAL y3 = y0 + G33;
            FN_DECIMAL z3 = z0 + G33;

            FN_DECIMAL n0, n1, n2, n3;

            t = (FN_DECIMAL) 0.6 - x0 * x0 - y0 * y0 - z0 * z0;

            if (t < 0) n0 = 0;
            else
            {
                t *= t;
                n0 = t * t * GradCoord3D(seed, i, j, k, x0, y0, z0);
            }

            t = (FN_DECIMAL) 0.6 - x1 * x1 - y1 * y1 - z1 * z1;

            if (t < 0) n1 = 0;
            else
            {
                t *= t;
                n1 = t * t * GradCoord3D(seed, i + i1, j + j1, k + k1, x1, y1, z1);
            }

            t = (FN_DECIMAL) 0.6 - x2 * x2 - y2 * y2 - z2 * z2;

            if (t < 0) n2 = 0;
            else
            {
                t *= t;
                n2 = t * t * GradCoord3D(seed, i + i2, j + j2, k + k2, x2, y2, z2);
            }

            t = (FN_DECIMAL) 0.6 - x3 * x3 - y3 * y3 - z3 * z3;

            if (t < 0) n3 = 0;
            else
            {
                t *= t;
                n3 = t * t * GradCoord3D(seed, i + 1, j + 1, k + 1, x3, y3, z3);
            }

            return 32 * (n0 + n1 + n2 + n3);
        }

        public FN_DECIMAL GetSimplexFractal(FN_DECIMAL x, FN_DECIMAL y)
        {
            x *= m_frequency;
            y *= m_frequency;

            switch (m_fractalType)
            {
                case FractalType.FBM:
                    return SingleSimplexFractalFBM(x, y);

                case FractalType.Billow:
                    return SingleSimplexFractalBillow(x, y);

                case FractalType.RigidMulti:
                    return SingleSimplexFractalRigidMulti(x, y);

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SingleSimplexFractalFBM(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = SingleSimplex(seed, x, y);
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum += SingleSimplex(++seed, x, y) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleSimplexFractalBillow(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = Math.Abs(SingleSimplex(seed, x, y)) * 2 - 1;
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum += (Math.Abs(SingleSimplex(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleSimplexFractalRigidMulti(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = 1 - Math.Abs(SingleSimplex(seed, x, y));
            FN_DECIMAL amp = 1;

            for (var i = 1; i < m_octaves; i++)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum -= (1 - Math.Abs(SingleSimplex(++seed, x, y))) * amp;
            }

            return sum;
        }

        public FN_DECIMAL GetSimplex(FN_DECIMAL x, FN_DECIMAL y)
        {
            return SingleSimplex(m_seed, x * m_frequency, y * m_frequency);
        }

        private static FN_DECIMAL SingleSimplex(int seed, FN_DECIMAL x, FN_DECIMAL y)
        {
            FN_DECIMAL t = (x + y) * F2;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);

            t = (i + j) * G2;
            FN_DECIMAL X0 = i - t;
            FN_DECIMAL Y0 = j - t;

            FN_DECIMAL x0 = x - X0;
            FN_DECIMAL y0 = y - Y0;

            int i1, j1;

            if (x0 > y0)
            {
                i1 = 1;
                j1 = 0;
            }
            else
            {
                i1 = 0;
                j1 = 1;
            }

            FN_DECIMAL x1 = x0 - i1 + G2;
            FN_DECIMAL y1 = y0 - j1 + G2;
            FN_DECIMAL x2 = x0 - 1 + F2;
            FN_DECIMAL y2 = y0 - 1 + F2;

            FN_DECIMAL n0, n1, n2;

            t = (FN_DECIMAL) 0.5 - x0 * x0 - y0 * y0;

            if (t < 0) n0 = 0;
            else
            {
                t *= t;
                n0 = t * t * GradCoord2D(seed, i, j, x0, y0);
            }

            t = (FN_DECIMAL) 0.5 - x1 * x1 - y1 * y1;

            if (t < 0) n1 = 0;
            else
            {
                t *= t;
                n1 = t * t * GradCoord2D(seed, i + i1, j + j1, x1, y1);
            }

            t = (FN_DECIMAL) 0.5 - x2 * x2 - y2 * y2;

            if (t < 0) n2 = 0;
            else
            {
                t *= t;
                n2 = t * t * GradCoord2D(seed, i + 1, j + 1, x2, y2);
            }

            return 50 * (n0 + n1 + n2);
        }

        public FN_DECIMAL GetSimplex(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z, FN_DECIMAL w)
        {
            return SingleSimplex(m_seed, x * m_frequency, y * m_frequency, z * m_frequency, w * m_frequency);
        }

        private static FN_DECIMAL SingleSimplex(int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z, FN_DECIMAL w)
        {
            FN_DECIMAL n0, n1, n2, n3, n4;
            FN_DECIMAL t = (x + y + z + w) * F4;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);
            int k = FastFloor(z + t);
            int l = FastFloor(w + t);
            t = (i + j + k + l) * G4;
            FN_DECIMAL X0 = i - t;
            FN_DECIMAL Y0 = j - t;
            FN_DECIMAL Z0 = k - t;
            FN_DECIMAL W0 = l - t;
            FN_DECIMAL x0 = x - X0;
            FN_DECIMAL y0 = y - Y0;
            FN_DECIMAL z0 = z - Z0;
            FN_DECIMAL w0 = w - W0;

            int c = (x0 > y0) ? 32 : 0;
            c += (x0 > z0) ? 16 : 0;
            c += (y0 > z0) ? 8 : 0;
            c += (x0 > w0) ? 4 : 0;
            c += (y0 > w0) ? 2 : 0;
            c += (z0 > w0) ? 1 : 0;
            c <<= 2;

            int i1 = SIMPLEX_4D[c] >= 3 ? 1 : 0;
            int i2 = SIMPLEX_4D[c] >= 2 ? 1 : 0;
            int i3 = SIMPLEX_4D[c++] >= 1 ? 1 : 0;
            int j1 = SIMPLEX_4D[c] >= 3 ? 1 : 0;
            int j2 = SIMPLEX_4D[c] >= 2 ? 1 : 0;
            int j3 = SIMPLEX_4D[c++] >= 1 ? 1 : 0;
            int k1 = SIMPLEX_4D[c] >= 3 ? 1 : 0;
            int k2 = SIMPLEX_4D[c] >= 2 ? 1 : 0;
            int k3 = SIMPLEX_4D[c++] >= 1 ? 1 : 0;
            int l1 = SIMPLEX_4D[c] >= 3 ? 1 : 0;
            int l2 = SIMPLEX_4D[c] >= 2 ? 1 : 0;
            int l3 = SIMPLEX_4D[c] >= 1 ? 1 : 0;

            FN_DECIMAL x1 = x0 - i1 + G4;
            FN_DECIMAL y1 = y0 - j1 + G4;
            FN_DECIMAL z1 = z0 - k1 + G4;
            FN_DECIMAL w1 = w0 - l1 + G4;
            FN_DECIMAL x2 = x0 - i2 + 2 * G4;
            FN_DECIMAL y2 = y0 - j2 + 2 * G4;
            FN_DECIMAL z2 = z0 - k2 + 2 * G4;
            FN_DECIMAL w2 = w0 - l2 + 2 * G4;
            FN_DECIMAL x3 = x0 - i3 + 3 * G4;
            FN_DECIMAL y3 = y0 - j3 + 3 * G4;
            FN_DECIMAL z3 = z0 - k3 + 3 * G4;
            FN_DECIMAL w3 = w0 - l3 + 3 * G4;
            FN_DECIMAL x4 = x0 - 1 + 4 * G4;
            FN_DECIMAL y4 = y0 - 1 + 4 * G4;
            FN_DECIMAL z4 = z0 - 1 + 4 * G4;
            FN_DECIMAL w4 = w0 - 1 + 4 * G4;

            t = (FN_DECIMAL) 0.6 - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;

            if (t < 0) n0 = 0;
            else
            {
                t *= t;
                n0 = t * t * GradCoord4D(seed, i, j, k, l, x0, y0, z0, w0);
            }

            t = (FN_DECIMAL) 0.6 - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;

            if (t < 0) n1 = 0;
            else
            {
                t *= t;
                n1 = t * t * GradCoord4D(seed, i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1);
            }

            t = (FN_DECIMAL) 0.6 - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;

            if (t < 0) n2 = 0;
            else
            {
                t *= t;
                n2 = t * t * GradCoord4D(seed, i + i2, j + j2, k + k2, l + l2, x2, y2, z2, w2);
            }

            t = (FN_DECIMAL) 0.6 - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;

            if (t < 0) n3 = 0;
            else
            {
                t *= t;
                n3 = t * t * GradCoord4D(seed, i + i3, j + j3, k + k3, l + l3, x3, y3, z3, w3);
            }

            t = (FN_DECIMAL) 0.6 - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;

            if (t < 0) n4 = 0;
            else
            {
                t *= t;
                n4 = t * t * GradCoord4D(seed, i + 1, j + 1, k + 1, l + 1, x4, y4, z4, w4);
            }

            return 27 * (n0 + n1 + n2 + n3 + n4);
        }

        // Cubic Noise
        public FN_DECIMAL GetCubicFractal(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            x *= m_frequency;
            y *= m_frequency;
            z *= m_frequency;

            switch (m_fractalType)
            {
                case FractalType.FBM:
                    return SingleCubicFractalFBM(x, y, z);

                case FractalType.Billow:
                    return SingleCubicFractalBillow(x, y, z);

                case FractalType.RigidMulti:
                    return SingleCubicFractalRigidMulti(x, y, z);

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SingleCubicFractalFBM(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = SingleCubic(seed, x, y, z);
            FN_DECIMAL amp = 1;
            var i = 0;

            while (++i < m_octaves)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum += SingleCubic(++seed, x, y, z) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleCubicFractalBillow(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = Math.Abs(SingleCubic(seed, x, y, z)) * 2 - 1;
            FN_DECIMAL amp = 1;
            var i = 0;

            while (++i < m_octaves)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum += (Math.Abs(SingleCubic(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleCubicFractalRigidMulti(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL sum = 1 - Math.Abs(SingleCubic(seed, x, y, z));
            FN_DECIMAL amp = 1;
            var i = 0;

            while (++i < m_octaves)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;
                z *= m_lacunarity;

                amp *= m_gain;
                sum -= (1 - Math.Abs(SingleCubic(++seed, x, y, z))) * amp;
            }

            return sum;
        }

        public FN_DECIMAL GetCubic(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            return SingleCubic(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
        }

        private static FN_DECIMAL SingleCubic(int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int x1 = FastFloor(x);
            int y1 = FastFloor(y);
            int z1 = FastFloor(z);

            int x0 = x1 - 1;
            int y0 = y1 - 1;
            int z0 = z1 - 1;
            int x2 = x1 + 1;
            int y2 = y1 + 1;
            int z2 = z1 + 1;
            int x3 = x1 + 2;
            int y3 = y1 + 2;
            int z3 = z1 + 2;

            FN_DECIMAL xs = x - (FN_DECIMAL) x1;
            FN_DECIMAL ys = y - (FN_DECIMAL) y1;
            FN_DECIMAL zs = z - (FN_DECIMAL) z1;

            return CubicLerp(
                CubicLerp(
                    CubicLerp(
                        ValCoord3D(seed, x0, y0, z0),
                        ValCoord3D(seed, x1, y0, z0),
                        ValCoord3D(seed, x2, y0, z0),
                        ValCoord3D(seed, x3, y0, z0),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y1, z0),
                        ValCoord3D(seed, x1, y1, z0),
                        ValCoord3D(seed, x2, y1, z0),
                        ValCoord3D(seed, x3, y1, z0),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y2, z0),
                        ValCoord3D(seed, x1, y2, z0),
                        ValCoord3D(seed, x2, y2, z0),
                        ValCoord3D(seed, x3, y2, z0),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y3, z0),
                        ValCoord3D(seed, x1, y3, z0),
                        ValCoord3D(seed, x2, y3, z0),
                        ValCoord3D(seed, x3, y3, z0),
                        xs),
                    ys),
                CubicLerp(
                    CubicLerp(
                        ValCoord3D(seed, x0, y0, z1),
                        ValCoord3D(seed, x1, y0, z1),
                        ValCoord3D(seed, x2, y0, z1),
                        ValCoord3D(seed, x3, y0, z1),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y1, z1),
                        ValCoord3D(seed, x1, y1, z1),
                        ValCoord3D(seed, x2, y1, z1),
                        ValCoord3D(seed, x3, y1, z1),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y2, z1),
                        ValCoord3D(seed, x1, y2, z1),
                        ValCoord3D(seed, x2, y2, z1),
                        ValCoord3D(seed, x3, y2, z1),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y3, z1),
                        ValCoord3D(seed, x1, y3, z1),
                        ValCoord3D(seed, x2, y3, z1),
                        ValCoord3D(seed, x3, y3, z1),
                        xs),
                    ys),
                CubicLerp(
                    CubicLerp(
                        ValCoord3D(seed, x0, y0, z2),
                        ValCoord3D(seed, x1, y0, z2),
                        ValCoord3D(seed, x2, y0, z2),
                        ValCoord3D(seed, x3, y0, z2),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y1, z2),
                        ValCoord3D(seed, x1, y1, z2),
                        ValCoord3D(seed, x2, y1, z2),
                        ValCoord3D(seed, x3, y1, z2),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y2, z2),
                        ValCoord3D(seed, x1, y2, z2),
                        ValCoord3D(seed, x2, y2, z2),
                        ValCoord3D(seed, x3, y2, z2),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y3, z2),
                        ValCoord3D(seed, x1, y3, z2),
                        ValCoord3D(seed, x2, y3, z2),
                        ValCoord3D(seed, x3, y3, z2),
                        xs),
                    ys),
                CubicLerp(
                    CubicLerp(
                        ValCoord3D(seed, x0, y0, z3),
                        ValCoord3D(seed, x1, y0, z3),
                        ValCoord3D(seed, x2, y0, z3),
                        ValCoord3D(seed, x3, y0, z3),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y1, z3),
                        ValCoord3D(seed, x1, y1, z3),
                        ValCoord3D(seed, x2, y1, z3),
                        ValCoord3D(seed, x3, y1, z3),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y2, z3),
                        ValCoord3D(seed, x1, y2, z3),
                        ValCoord3D(seed, x2, y2, z3),
                        ValCoord3D(seed, x3, y2, z3),
                        xs),
                    CubicLerp(
                        ValCoord3D(seed, x0, y3, z3),
                        ValCoord3D(seed, x1, y3, z3),
                        ValCoord3D(seed, x2, y3, z3),
                        ValCoord3D(seed, x3, y3, z3),
                        xs),
                    ys),
                zs) * CUBIC_3D_BOUNDING;
        }

        public FN_DECIMAL GetCubicFractal(FN_DECIMAL x, FN_DECIMAL y)
        {
            x *= m_frequency;
            y *= m_frequency;

            switch (m_fractalType)
            {
                case FractalType.FBM:
                    return SingleCubicFractalFBM(x, y);

                case FractalType.Billow:
                    return SingleCubicFractalBillow(x, y);

                case FractalType.RigidMulti:
                    return SingleCubicFractalRigidMulti(x, y);

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SingleCubicFractalFBM(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = SingleCubic(seed, x, y);
            FN_DECIMAL amp = 1;
            var i = 0;

            while (++i < m_octaves)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum += SingleCubic(++seed, x, y) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleCubicFractalBillow(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = Math.Abs(SingleCubic(seed, x, y)) * 2 - 1;
            FN_DECIMAL amp = 1;
            var i = 0;

            while (++i < m_octaves)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum += (Math.Abs(SingleCubic(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * m_fractalBounding;
        }

        private FN_DECIMAL SingleCubicFractalRigidMulti(FN_DECIMAL x, FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL sum = 1 - Math.Abs(SingleCubic(seed, x, y));
            FN_DECIMAL amp = 1;
            var i = 0;

            while (++i < m_octaves)
            {
                x *= m_lacunarity;
                y *= m_lacunarity;

                amp *= m_gain;
                sum -= (1 - Math.Abs(SingleCubic(++seed, x, y))) * amp;
            }

            return sum;
        }

        public FN_DECIMAL GetCubic(FN_DECIMAL x, FN_DECIMAL y)
        {
            x *= m_frequency;
            y *= m_frequency;

            return SingleCubic(seed: 0, x, y);
        }

        private static FN_DECIMAL SingleCubic(int seed, FN_DECIMAL x, FN_DECIMAL y)
        {
            int x1 = FastFloor(x);
            int y1 = FastFloor(y);

            int x0 = x1 - 1;
            int y0 = y1 - 1;
            int x2 = x1 + 1;
            int y2 = y1 + 1;
            int x3 = x1 + 2;
            int y3 = y1 + 2;

            FN_DECIMAL xs = x - (FN_DECIMAL) x1;
            FN_DECIMAL ys = y - (FN_DECIMAL) y1;

            return CubicLerp(
                CubicLerp(
                    ValCoord2D(seed, x0, y0),
                    ValCoord2D(seed, x1, y0),
                    ValCoord2D(seed, x2, y0),
                    ValCoord2D(seed, x3, y0),
                    xs),
                CubicLerp(
                    ValCoord2D(seed, x0, y1),
                    ValCoord2D(seed, x1, y1),
                    ValCoord2D(seed, x2, y1),
                    ValCoord2D(seed, x3, y1),
                    xs),
                CubicLerp(
                    ValCoord2D(seed, x0, y2),
                    ValCoord2D(seed, x1, y2),
                    ValCoord2D(seed, x2, y2),
                    ValCoord2D(seed, x3, y2),
                    xs),
                CubicLerp(
                    ValCoord2D(seed, x0, y3),
                    ValCoord2D(seed, x1, y3),
                    ValCoord2D(seed, x2, y3),
                    ValCoord2D(seed, x3, y3),
                    xs),
                ys) * CUBIC_2D_BOUNDING;
        }

        // Cellular Noise
        public FN_DECIMAL GetCellular(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            x *= m_frequency;
            y *= m_frequency;
            z *= m_frequency;

            switch (m_cellularReturnType)
            {
                case CellularReturnType.CellValue:
                case CellularReturnType.NoiseLookup:
                case CellularReturnType.Distance:
                    return SingleCellular(x, y, z);

                default:
                    return SingleCellular2Edge(x, y, z);
            }
        }

        private FN_DECIMAL SingleCellular(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);
            int zr = FastRound(z);

            FN_DECIMAL distance = 999999;
            int xc = 0, yc = 0, zc = 0;

            switch (m_cellularDistanceFunction)
            {
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Float3 vec = CELL_3D[Hash3D(m_seed, xi, yi, zi) & 255];

                                FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                                FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
                                FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

                                FN_DECIMAL newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }

                    break;

                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Float3 vec = CELL_3D[Hash3D(m_seed, xi, yi, zi) & 255];

                                FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                                FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
                                FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

                                FN_DECIMAL newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ);

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }

                    break;

                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Float3 vec = CELL_3D[Hash3D(m_seed, xi, yi, zi) & 255];

                                FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                                FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
                                FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

                                FN_DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ)) +
                                                         (vecX * vecX + vecY * vecY + vecZ * vecZ);

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }

                    break;
            }

            switch (m_cellularReturnType)
            {
                case CellularReturnType.CellValue:
                    return ValCoord3D(m_seed, xc, yc, zc);

                case CellularReturnType.NoiseLookup:
                    Float3 vec = CELL_3D[Hash3D(m_seed, xc, yc, zc) & 255];

                    return m_cellularNoiseLookup.GetNoise(
                        xc + vec.x * m_cellularJitter,
                        yc + vec.y * m_cellularJitter,
                        zc + vec.z * m_cellularJitter);

                case CellularReturnType.Distance:
                    return distance;

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SingleCellular2Edge(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);
            int zr = FastRound(z);

            FN_DECIMAL[] distance = { 999999, 999999, 999999, 999999 };

            switch (m_cellularDistanceFunction)
            {
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Float3 vec = CELL_3D[Hash3D(m_seed, xi, yi, zi) & 255];

                                FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                                FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
                                FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

                                FN_DECIMAL newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                                for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);

                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }

                    break;

                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Float3 vec = CELL_3D[Hash3D(m_seed, xi, yi, zi) & 255];

                                FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                                FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
                                FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

                                FN_DECIMAL newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ);

                                for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);

                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }

                    break;

                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Float3 vec = CELL_3D[Hash3D(m_seed, xi, yi, zi) & 255];

                                FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                                FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
                                FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

                                FN_DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ)) +
                                                         (vecX * vecX + vecY * vecY + vecZ * vecZ);

                                for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);

                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }

                    break;

                default:
                    break;
            }

            switch (m_cellularReturnType)
            {
                case CellularReturnType.Distance2:
                    return distance[m_cellularDistanceIndex1];

                case CellularReturnType.Distance2Add:
                    return distance[m_cellularDistanceIndex1] + distance[m_cellularDistanceIndex0];

                case CellularReturnType.Distance2Sub:
                    return distance[m_cellularDistanceIndex1] - distance[m_cellularDistanceIndex0];

                case CellularReturnType.Distance2Mul:
                    return distance[m_cellularDistanceIndex1] * distance[m_cellularDistanceIndex0];

                case CellularReturnType.Distance2Div:
                    return distance[m_cellularDistanceIndex0] / distance[m_cellularDistanceIndex1];

                default:
                    return 0;
            }
        }

        public FN_DECIMAL GetCellular(FN_DECIMAL x, FN_DECIMAL y)
        {
            x *= m_frequency;
            y *= m_frequency;

            switch (m_cellularReturnType)
            {
                case CellularReturnType.CellValue:
                case CellularReturnType.NoiseLookup:
                case CellularReturnType.Distance:
                    return SingleCellular(x, y);

                default:
                    return SingleCellular2Edge(x, y);
            }
        }

        private FN_DECIMAL SingleCellular(FN_DECIMAL x, FN_DECIMAL y)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);

            FN_DECIMAL distance = 999999;
            int xc = 0, yc = 0;

            switch (m_cellularDistanceFunction)
            {
                default:
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Float2 vec = CELL_2D[Hash2D(m_seed, xi, yi) & 255];

                            FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                            FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

                            FN_DECIMAL newDistance = vecX * vecX + vecY * vecY;

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }

                    break;

                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Float2 vec = CELL_2D[Hash2D(m_seed, xi, yi) & 255];

                            FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                            FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

                            FN_DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY));

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }

                    break;

                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Float2 vec = CELL_2D[Hash2D(m_seed, xi, yi) & 255];

                            FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                            FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

                            FN_DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY)) + (vecX * vecX + vecY * vecY);

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }

                    break;
            }

            switch (m_cellularReturnType)
            {
                case CellularReturnType.CellValue:
                    return ValCoord2D(m_seed, xc, yc);

                case CellularReturnType.NoiseLookup:
                    Float2 vec = CELL_2D[Hash2D(m_seed, xc, yc) & 255];

                    return m_cellularNoiseLookup.GetNoise(xc + vec.x * m_cellularJitter, yc + vec.y * m_cellularJitter);

                case CellularReturnType.Distance:
                    return distance;

                default:
                    return 0;
            }
        }

        private FN_DECIMAL SingleCellular2Edge(FN_DECIMAL x, FN_DECIMAL y)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);

            FN_DECIMAL[] distance = { 999999, 999999, 999999, 999999 };

            switch (m_cellularDistanceFunction)
            {
                default:
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Float2 vec = CELL_2D[Hash2D(m_seed, xi, yi) & 255];

                            FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                            FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

                            FN_DECIMAL newDistance = vecX * vecX + vecY * vecY;

                            for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);

                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }

                    break;

                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Float2 vec = CELL_2D[Hash2D(m_seed, xi, yi) & 255];

                            FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                            FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

                            FN_DECIMAL newDistance = Math.Abs(vecX) + Math.Abs(vecY);

                            for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);

                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }

                    break;

                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Float2 vec = CELL_2D[Hash2D(m_seed, xi, yi) & 255];

                            FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
                            FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

                            FN_DECIMAL newDistance = (Math.Abs(vecX) + Math.Abs(vecY)) + (vecX * vecX + vecY * vecY);

                            for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);

                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }

                    break;
            }

            switch (m_cellularReturnType)
            {
                case CellularReturnType.Distance2:
                    return distance[m_cellularDistanceIndex1];

                case CellularReturnType.Distance2Add:
                    return distance[m_cellularDistanceIndex1] + distance[m_cellularDistanceIndex0];

                case CellularReturnType.Distance2Sub:
                    return distance[m_cellularDistanceIndex1] - distance[m_cellularDistanceIndex0];

                case CellularReturnType.Distance2Mul:
                    return distance[m_cellularDistanceIndex1] * distance[m_cellularDistanceIndex0];

                case CellularReturnType.Distance2Div:
                    return distance[m_cellularDistanceIndex0] / distance[m_cellularDistanceIndex1];

                default:
                    return 0;
            }
        }

        public void GradientPerturb(ref FN_DECIMAL x, ref FN_DECIMAL y, ref FN_DECIMAL z)
        {
            SingleGradientPerturb(m_seed, m_gradientPerturbAmp, m_frequency, ref x, ref y, ref z);
        }

        public void GradientPerturbFractal(ref FN_DECIMAL x, ref FN_DECIMAL y, ref FN_DECIMAL z)
        {
            int seed = m_seed;
            FN_DECIMAL amp = m_gradientPerturbAmp * m_fractalBounding;
            FN_DECIMAL freq = m_frequency;

            SingleGradientPerturb(seed, amp, m_frequency, ref x, ref y, ref z);

            for (var i = 1; i < m_octaves; i++)
            {
                freq *= m_lacunarity;
                amp *= m_gain;
                SingleGradientPerturb(++seed, amp, freq, ref x, ref y, ref z);
            }
        }

        private void SingleGradientPerturb(int seed, FN_DECIMAL perturbAmp, FN_DECIMAL frequency, ref FN_DECIMAL x,
            ref FN_DECIMAL y, ref FN_DECIMAL z)
        {
            FN_DECIMAL xf = x * frequency;
            FN_DECIMAL yf = y * frequency;
            FN_DECIMAL zf = z * frequency;

            int x0 = FastFloor(xf);
            int y0 = FastFloor(yf);
            int z0 = FastFloor(zf);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;

            FN_DECIMAL xs, ys, zs;

            switch (m_interp)
            {
                default:
                case Interp.Linear:
                    xs = xf - x0;
                    ys = yf - y0;
                    zs = zf - z0;

                    break;

                case Interp.Hermite:
                    xs = InterpHermiteFunc(xf - x0);
                    ys = InterpHermiteFunc(yf - y0);
                    zs = InterpHermiteFunc(zf - z0);

                    break;

                case Interp.Quintic:
                    xs = InterpQuinticFunc(xf - x0);
                    ys = InterpQuinticFunc(yf - y0);
                    zs = InterpQuinticFunc(zf - z0);

                    break;
            }

            Float3 vec0 = CELL_3D[Hash3D(seed, x0, y0, z0) & 255];
            Float3 vec1 = CELL_3D[Hash3D(seed, x1, y0, z0) & 255];

            FN_DECIMAL lx0x = Lerp(vec0.x, vec1.x, xs);
            FN_DECIMAL ly0x = Lerp(vec0.y, vec1.y, xs);
            FN_DECIMAL lz0x = Lerp(vec0.z, vec1.z, xs);

            vec0 = CELL_3D[Hash3D(seed, x0, y1, z0) & 255];
            vec1 = CELL_3D[Hash3D(seed, x1, y1, z0) & 255];

            FN_DECIMAL lx1x = Lerp(vec0.x, vec1.x, xs);
            FN_DECIMAL ly1x = Lerp(vec0.y, vec1.y, xs);
            FN_DECIMAL lz1x = Lerp(vec0.z, vec1.z, xs);

            FN_DECIMAL lx0y = Lerp(lx0x, lx1x, ys);
            FN_DECIMAL ly0y = Lerp(ly0x, ly1x, ys);
            FN_DECIMAL lz0y = Lerp(lz0x, lz1x, ys);

            vec0 = CELL_3D[Hash3D(seed, x0, y0, z1) & 255];
            vec1 = CELL_3D[Hash3D(seed, x1, y0, z1) & 255];

            lx0x = Lerp(vec0.x, vec1.x, xs);
            ly0x = Lerp(vec0.y, vec1.y, xs);
            lz0x = Lerp(vec0.z, vec1.z, xs);

            vec0 = CELL_3D[Hash3D(seed, x0, y1, z1) & 255];
            vec1 = CELL_3D[Hash3D(seed, x1, y1, z1) & 255];

            lx1x = Lerp(vec0.x, vec1.x, xs);
            ly1x = Lerp(vec0.y, vec1.y, xs);
            lz1x = Lerp(vec0.z, vec1.z, xs);

            x += Lerp(lx0y, Lerp(lx0x, lx1x, ys), zs) * perturbAmp;
            y += Lerp(ly0y, Lerp(ly0x, ly1x, ys), zs) * perturbAmp;
            z += Lerp(lz0y, Lerp(lz0x, lz1x, ys), zs) * perturbAmp;
        }

        public void GradientPerturb(ref FN_DECIMAL x, ref FN_DECIMAL y)
        {
            SingleGradientPerturb(m_seed, m_gradientPerturbAmp, m_frequency, ref x, ref y);
        }

        public void GradientPerturbFractal(ref FN_DECIMAL x, ref FN_DECIMAL y)
        {
            int seed = m_seed;
            FN_DECIMAL amp = m_gradientPerturbAmp * m_fractalBounding;
            FN_DECIMAL freq = m_frequency;

            SingleGradientPerturb(seed, amp, m_frequency, ref x, ref y);

            for (var i = 1; i < m_octaves; i++)
            {
                freq *= m_lacunarity;
                amp *= m_gain;
                SingleGradientPerturb(++seed, amp, freq, ref x, ref y);
            }
        }

        private void SingleGradientPerturb(int seed, FN_DECIMAL perturbAmp, FN_DECIMAL frequency, ref FN_DECIMAL x,
            ref FN_DECIMAL y)
        {
            FN_DECIMAL xf = x * frequency;
            FN_DECIMAL yf = y * frequency;

            int x0 = FastFloor(xf);
            int y0 = FastFloor(yf);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            FN_DECIMAL xs, ys;

            switch (m_interp)
            {
                default:
                case Interp.Linear:
                    xs = xf - x0;
                    ys = yf - y0;

                    break;

                case Interp.Hermite:
                    xs = InterpHermiteFunc(xf - x0);
                    ys = InterpHermiteFunc(yf - y0);

                    break;

                case Interp.Quintic:
                    xs = InterpQuinticFunc(xf - x0);
                    ys = InterpQuinticFunc(yf - y0);

                    break;
            }

            Float2 vec0 = CELL_2D[Hash2D(seed, x0, y0) & 255];
            Float2 vec1 = CELL_2D[Hash2D(seed, x1, y0) & 255];

            FN_DECIMAL lx0x = Lerp(vec0.x, vec1.x, xs);
            FN_DECIMAL ly0x = Lerp(vec0.y, vec1.y, xs);

            vec0 = CELL_2D[Hash2D(seed, x0, y1) & 255];
            vec1 = CELL_2D[Hash2D(seed, x1, y1) & 255];

            FN_DECIMAL lx1x = Lerp(vec0.x, vec1.x, xs);
            FN_DECIMAL ly1x = Lerp(vec0.y, vec1.y, xs);

            x += Lerp(lx0x, lx1x, ys) * perturbAmp;
            y += Lerp(ly0x, ly1x, ys) * perturbAmp;
        }

        private struct Float2
        {
            public readonly FN_DECIMAL x, y;

            public Float2(FN_DECIMAL x, FN_DECIMAL y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private struct Float3
        {
            public readonly FN_DECIMAL x, y, z;

            public Float3(FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
    }
}

#pragma warning restore