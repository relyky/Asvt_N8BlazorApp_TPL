const path = require("path");
//const HtmlWebpackPlugin = require("html-webpack-plugin");
module.exports = {
  mode: "production", // development | production
  entry: {
    main: {
      import: "/src/_main.js",
      dependOn: 'shared',
    },
    rxcounter: {
      import: "/src/_rxcounter.js",
      dependOn: 'shared',
    },
    rxwebcamera: {
      import: "/src/_rxwebcamera.js",
      dependOn: 'shared',
    },
    rxphotocrop: {
      import: "/src/_rxphotocrop.js",
      dependOn: 'shared',
    },
    rxqrcodescanner: {
      import: "/src/_rxqrcodescanner.js",
      dependOn: 'shared',
    },
    shared: 'lodash',
  },
  output: {
    path: path.resolve(__dirname, "../Vista.Component/wwwroot"), // output folder, default:"dist"
    publicPath: "/",
    filename: "[name].bundle.js",
    //library: { // library 只支援單一模組的樣子？這樣不合用。
    //  name: 'reactWidgets',
    //  type: 'umd',
    //},
  },
  optimization: {
    runtimeChunk: 'single',
  },
  module: {
    rules: [
      {
        test: /\.?js$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader",
          options: {
            presets: ["@babel/preset-env", "@babel/preset-react"],
          },
        },
      },
      {
        test: /\.css$/,
        use: [
          "style-loader",
          "css-loader", // for styles
        ],
      },
    ],
  },
  //plugins: [
  //  new HtmlWebpackPlugin({
  //    template: "./src/index.html", // base html
  //  }),
  //],
};