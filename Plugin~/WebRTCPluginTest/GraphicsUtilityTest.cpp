#include "pch.h"

#include "GraphicsDevice/GraphicsUtility.h"

#if SUPPORT_VULKAN
#include "GraphicsDevice/Vulkan/VulkanGraphicsDevice.h"
#include "UnityVulkanInterfaceFunctions.h"
#endif

namespace unity
{
namespace webrtc
{
    TEST(GraphicsUtilityTest, BorrowedTextureHandleIsNotDeleted)
    {
        int textureHandle = 42;
        auto native = GraphicsUtility::TextureHandleToNativeGraphicsPtr(&textureHandle, nullptr, kUnityGfxRendererNull);

        EXPECT_EQ(native.get(), &textureHandle);
        native.reset();
        EXPECT_EQ(textureHandle, 42);
    }

#if SUPPORT_VULKAN
    class MockVulkanImageAccess : public UnityGraphicsVulkan
    {
    public:
        MOCK_METHOD(void, EnsureOutsideRenderPass, (), (override));
        MOCK_METHOD(void, EnsureInsideRenderPass, (), (override));
        MOCK_METHOD(
            bool,
            AccessTexture,
            (void*,
             const VkImageSubresource*,
             VkImageLayout,
             VkPipelineStageFlags,
             VkAccessFlags,
             UnityVulkanResourceAccessMode,
             UnityVulkanImage*),
            (override));
        MOCK_METHOD(bool, CommandRecordingState, (UnityVulkanRecordingState*, UnityVulkanGraphicsQueueAccess), (override));
        MOCK_METHOD(bool, InterceptInitialization, (UnityVulkanInitCallback, void*), (override));
        MOCK_METHOD(PFN_vkVoidFunction, InterceptVulkanAPI, (const char*, PFN_vkVoidFunction), (override));
        MOCK_METHOD(bool, AddInterceptInitialization, (UnityVulkanInitCallback, void*, int), (override));
        MOCK_METHOD(UnityVulkanInstance, Instance, (), (override));
        MOCK_METHOD(void, AccessQueue, (UnityRenderingEventAndData, int, void*, bool), (override));
        MOCK_METHOD(void, ConfigureEvent, (int, const UnityVulkanPluginEventConfig*), (override));
    };

    // No Vulkan driver is needed. Leak-sanitized runs also verify the per-frame descriptors are freed.
    TEST(GraphicsUtilityTest, VulkanDescriptorsHaveScopedOwnership)
    {
        testing::NiceMock<MockVulkanImageAccess> access;
        UnityVulkanInstance instance {};
        VulkanGraphicsDevice device(&access, &instance, kUnityGfxRendererVulkan, nullptr);
        UnityVulkanImage image {};
        image.extent.width = 128;
        int textureHandle = 42;
        EXPECT_CALL(access, AccessTexture(testing::_, testing::_, testing::_, testing::_, testing::_, testing::_, testing::_))
            .Times(100)
            .WillRepeatedly(testing::DoAll(testing::SetArgPointee<6>(image), testing::Return(true)));

        for (int i = 0; i < 100; ++i)
        {
            auto native = GraphicsUtility::TextureHandleToNativeGraphicsPtr(&textureHandle, &device, kUnityGfxRendererVulkan);
            ASSERT_NE(native, nullptr);
            EXPECT_EQ(static_cast<UnityVulkanImage*>(native.get())->extent.width, 128);
        }
        EXPECT_EQ(textureHandle, 42);
    }
#endif
} // namespace webrtc
} // namespace unity
