#include "pch.h"

#include <vector>
#include <absl/types/optional.h>

#include "Surface.h"
#include "NativeFrameBuffer.h"

namespace unity
{
namespace webrtc
{
    class VulkanSurface : public Surface
    {
    public:
        VulkanSurface(VkSurfaceKHR surface, VkDevice device, VkPhysicalDevice physicalDevice)
            : surface_(surface)
            , device_(device)
            , physicalDevice_(physicalDevice)
        {
            QueueFamilyIndices indices = FindQueueFamilies(physicalDevice_);
            uint32_t queueFamilyIndices[] = {indices.graphicsFamily.value(), indices.presentFamily.value()};

            SwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(physicalDevice_);
            VkSurfaceFormatKHR surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.formats);
            VkPresentModeKHR presentMode = ChooseSwapPresentMode(swapChainSupport.presentModes);
            swapchainExtent_ = ChooseSwapExtent(swapChainSupport.capabilities);
            uint32_t minImageCount = swapChainSupport.capabilities.minImageCount;

            // get queue.
            vkGetDeviceQueue(device_, indices.graphicsFamily.value(), 0, &graphicsQueue_);
            vkGetDeviceQueue(device_, indices.presentFamily.value(), 0, &presentQueue_);

            // create swapchain.
            VkSwapchainCreateInfoKHR createInfo = {};
            createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
            createInfo.surface = surface_;
            createInfo.minImageCount = minImageCount;
            createInfo.imageFormat = surfaceFormat.format;
            createInfo.imageColorSpace = surfaceFormat.colorSpace;
            createInfo.imageExtent = swapchainExtent_;
            createInfo.imageArrayLayers = 1;
            createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

            if (indices.graphicsFamily != indices.presentFamily)
            {
                createInfo.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
                createInfo.queueFamilyIndexCount = 2;
                createInfo.pQueueFamilyIndices = queueFamilyIndices;
            }
            else
            {
                createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
                createInfo.queueFamilyIndexCount = 0;
                createInfo.pQueueFamilyIndices = nullptr;
            }

            createInfo.preTransform = swapChainSupport.capabilities.currentTransform;
            createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
            createInfo.presentMode = presentMode;
            createInfo.clipped = VK_TRUE;
            createInfo.oldSwapchain = VK_NULL_HANDLE;
            VkResult result = vkCreateSwapchainKHR(device_, &createInfo, nullptr, &swapchain_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkCreateSwapchainKHR failed. result=" << result;
                return;
            }

            // get swapchain images.
            uint32_t imageCount = 0;
            result = vkGetSwapchainImagesKHR(device_, swapchain_, &imageCount, nullptr);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkGetSwapchainImagesKHR failed. result=" << result;
                return;
            }
            images_.resize(imageCount);
            result = vkGetSwapchainImagesKHR(device_, swapchain_, &imageCount, images_.data());
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkGetSwapchainImagesKHR failed. result=" << result;
                return;
            }

            // create swapchain imageview.
            imageViews_.resize(images_.size());
            for (size_t i = 0; i < imageViews_.size(); i++)
            {
                VkImageViewCreateInfo viewCreateInfo{};
                viewCreateInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
                viewCreateInfo.image = images_[i];
                viewCreateInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
                viewCreateInfo.format = surfaceFormat.format;
                viewCreateInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
                viewCreateInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
                viewCreateInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
                viewCreateInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
                viewCreateInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
                viewCreateInfo.subresourceRange.baseMipLevel = 0;
                viewCreateInfo.subresourceRange.levelCount = 1;
                viewCreateInfo.subresourceRange.baseArrayLayer = 0;
                viewCreateInfo.subresourceRange.layerCount = 1;
                result = vkCreateImageView(device, &viewCreateInfo, nullptr, &imageViews_[i]);
                if(result != VK_SUCCESS)
                {
                    RTC_LOG(LS_INFO) << "vkCreateImageView failed. result=" << result;
                    return;
                }
            }

            // create pipeline layout
            VkPipelineLayoutCreateInfo pipelineLayoutInfo{};
            pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
            pipelineLayoutInfo.setLayoutCount = 0; // Optional
            pipelineLayoutInfo.pSetLayouts = nullptr; // Optional
            pipelineLayoutInfo.pushConstantRangeCount = 0; // Optional
            pipelineLayoutInfo.pPushConstantRanges = nullptr; // Optional

            result = vkCreatePipelineLayout(device, &pipelineLayoutInfo, nullptr, &pipelineLayout_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkCreatePipelineLayout failed. result=" << result;
                return;
            }

            // create render pass.
            VkAttachmentDescription colorAttachment = {};
            colorAttachment.format = surfaceFormat.format;
            colorAttachment.samples = VK_SAMPLE_COUNT_1_BIT;
            colorAttachment.loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
            colorAttachment.storeOp = VK_ATTACHMENT_STORE_OP_STORE;
            colorAttachment.stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
            colorAttachment.stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
            colorAttachment.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
            colorAttachment.finalLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;

            VkAttachmentReference colorAttachmentRef = {};
            colorAttachmentRef.attachment = 0;
            colorAttachmentRef.layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

            VkSubpassDescription subpass = {};
            subpass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
            subpass.colorAttachmentCount = 1;
            subpass.pColorAttachments = &colorAttachmentRef;

            VkRenderPassCreateInfo renderPassInfo = {};
            renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
            renderPassInfo.attachmentCount = 1;
            renderPassInfo.pAttachments = &colorAttachment;
            renderPassInfo.subpassCount = 1;
            renderPassInfo.pSubpasses = &subpass;

            VkSubpassDependency dependency = {};
            dependency.srcSubpass = VK_SUBPASS_EXTERNAL;
            dependency.dstSubpass = 0;
            dependency.srcStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
            dependency.srcAccessMask = 0;
            dependency.dstStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
            dependency.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
            renderPassInfo.dependencyCount = 1;
            renderPassInfo.pDependencies = &dependency;

            result = vkCreateRenderPass(device, &renderPassInfo, nullptr, &renderPass_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkCreateRenderPass failed. result=" << result;
                return;
            }

            // create framebuffers.
            framebuffers_.resize(images_.size());
            for (size_t i = 0; i < framebuffers_.size(); i++)
            {
                std::array<VkImageView, 1> attachments = {imageViews_[i]};

                VkFramebufferCreateInfo framebufferInfo = {};
                framebufferInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
                framebufferInfo.renderPass = renderPass_;
                framebufferInfo.attachmentCount = static_cast<uint32_t>(attachments.size());
                framebufferInfo.pAttachments = attachments.data();
                framebufferInfo.width = swapchainExtent_.width;
                framebufferInfo.height = swapchainExtent_.height;
                framebufferInfo.layers = 1;

                result = vkCreateFramebuffer(
                        device_,
                        &framebufferInfo,
                        nullptr,
                        &framebuffers_[i]);
                if(result != VK_SUCCESS)
                {
                    RTC_LOG(LS_INFO) << "vkCreateFramebuffer failed. result=" << result;
                    return;
                }
            }

            // create command pool.
            VkCommandPoolCreateInfo poolInfo = {};
            poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
            poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
            poolInfo.queueFamilyIndex = indices.graphicsFamily.value();

            result = vkCreateCommandPool(device, &poolInfo, nullptr, &commandPool_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkCreateCommandPool failed. result=" << result;
                return;
            }

            // create command buffer.
            VkCommandBufferAllocateInfo allocInfo = {};
            allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
            allocInfo.commandPool = commandPool_;
            allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
            allocInfo.commandBufferCount = 1;

            result = vkAllocateCommandBuffers(device, &allocInfo, &commandBuffer_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkAllocateCommandBuffers failed. result=" << result;
                return;
            }

            // create two semaphores.
            VkSemaphoreCreateInfo semaphoreInfo = {};
            semaphoreInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;

            result = vkCreateSemaphore(device, &semaphoreInfo, nullptr, &imageAvailableSemaphore_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkCreateSemaphore failed. result=" << result;
                return;
            }
            result = vkCreateSemaphore(device, &semaphoreInfo, nullptr, &renderFinishedSemaphore_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkCreateSemaphore failed. result=" << result;
                return;
            }

            // create fence.
            VkFenceCreateInfo fenceInfo{};
            fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
            fenceInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;

            result = vkCreateFence(device, &fenceInfo, nullptr, &inFlightFence_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkCreateFence failed. result=" << result;
                return;
            }
        }
        ~VulkanSurface() override
        {
            vkDestroySwapchainKHR(device_, swapchain_, nullptr);
            for (auto imageView : imageViews_)
            {
                vkDestroyImageView(device_, imageView, nullptr);
            }
            vkDestroyPipelineLayout(device_, pipelineLayout_, nullptr);
            vkDestroyRenderPass(device_, renderPass_, nullptr);
            for(auto framebuffer : framebuffers_)
            {
                vkDestroyFramebuffer(device_, framebuffer, nullptr);
            }
            vkDestroyCommandPool(device_, commandPool_, nullptr);

            vkDestroySemaphore(device_, imageAvailableSemaphore_, nullptr);
            vkDestroySemaphore(device_, renderFinishedSemaphore_, nullptr);
            vkDestroyFence(device_, inFlightFence_, nullptr);
        }

        void DrawFrame(const NativeFrameBuffer* buffer) override
        {
            // wait for fence.
            VkResult result = vkWaitForFences(device_, 1, &inFlightFence_, VK_TRUE, UINT64_MAX);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkWaitForFences failed. result=" << result;
                return;
            }

            // reset fence.
            result = vkResetFences(device_, 1, &inFlightFence_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkResetFences failed. result=" << result;
                return;
            }

            // get next image index.
            uint32_t imageIndex;
            result = vkAcquireNextImageKHR(device_, swapchain_, UINT64_MAX, imageAvailableSemaphore_, VK_NULL_HANDLE, &imageIndex);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkAcquireNextImageKHR failed. result=" << result;
                return;
            }

            // reset command buffer.
            result = vkResetCommandBuffer(commandBuffer_, 0);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkResetCommandBuffer failed. result=" << result;
                return;
            }

            // record command buffer.
            result = RecordCommandBuffer(buffer, commandBuffer_, imageIndex);
            if(result != VK_SUCCESS)
                return;

            // submit the command buffer.
            VkSubmitInfo submitInfo{};
            submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;

            VkSemaphore waitSemaphores[] = {imageAvailableSemaphore_ };
            VkPipelineStageFlags waitStages[] = {VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT };
            submitInfo.waitSemaphoreCount = 1;
            submitInfo.pWaitSemaphores = waitSemaphores;
            submitInfo.pWaitDstStageMask = waitStages;
            submitInfo.commandBufferCount = 1;
            submitInfo.pCommandBuffers = &commandBuffer_;

            VkSemaphore signalSemaphores[] = { renderFinishedSemaphore_ };
            submitInfo.signalSemaphoreCount = 1;
            submitInfo.pSignalSemaphores = signalSemaphores;

            result = vkQueueSubmit(graphicsQueue_, 1, &submitInfo, inFlightFence_);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkQueueSubmit failed. result=" << result;
                return;
            }

            // present queue
            VkPresentInfoKHR presentInfo{};
            presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
            presentInfo.waitSemaphoreCount = 1;
            presentInfo.pWaitSemaphores = signalSemaphores;

            VkSwapchainKHR swapChains[] = {swapchain_ };
            presentInfo.swapchainCount = 1;
            presentInfo.pSwapchains = swapChains;
            presentInfo.pImageIndices = &imageIndex;

            result = vkQueuePresentKHR(presentQueue_, &presentInfo);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkQueueSubmit failed. result=" << result;
                return;
            }
        }

    private:
        VkQueue graphicsQueue_;
        VkQueue presentQueue_;
        VkSurfaceKHR surface_;
        VkDevice device_;
        VkPhysicalDevice physicalDevice_;
        VkSwapchainKHR swapchain_;
        VkExtent2D swapchainExtent_;
        std::vector<VkImage> images_;
        std::vector<VkImageView> imageViews_;
        VkRenderPass renderPass_;
        VkPipelineLayout pipelineLayout_;
        std::vector<VkFramebuffer> framebuffers_;
        VkCommandPool commandPool_;
        VkCommandBuffer commandBuffer_;
        VkSemaphore imageAvailableSemaphore_;
        VkSemaphore renderFinishedSemaphore_;
        VkFence inFlightFence_;

        struct SwapChainSupportDetails
        {
            VkSurfaceCapabilitiesKHR capabilities;
            std::vector<VkSurfaceFormatKHR> formats;
            std::vector<VkPresentModeKHR> presentModes;
        };

        struct QueueFamilyIndices
        {
            absl::optional<uint32_t> graphicsFamily;
            absl::optional<uint32_t> presentFamily;

            bool isComplete()
            {
                return graphicsFamily.has_value() && presentFamily.has_value();
            }
        };

        QueueFamilyIndices FindQueueFamilies(VkPhysicalDevice device)
        {
            QueueFamilyIndices indices;
            uint32_t queueFamilyCount = 0;
            vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, nullptr);

            std::vector<VkQueueFamilyProperties> queueFamilies(queueFamilyCount);
            vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, queueFamilies.data());

            int i = 0;
            for (const auto& queueFamily : queueFamilies)
            {
                if (queueFamily.queueFlags & VK_QUEUE_GRAPHICS_BIT)
                    indices.graphicsFamily = i;

                VkBool32 presentSupport = false;
                vkGetPhysicalDeviceSurfaceSupportKHR(device, i, surface_, &presentSupport);
                if (presentSupport)
                    indices.presentFamily = i;

                if (indices.isComplete())
                    break;
                i++;
            }
            return indices;
        }

        SwapChainSupportDetails QuerySwapChainSupport(VkPhysicalDevice device)
        {
            SwapChainSupportDetails details;

            VkResult result = vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device, surface_, &details.capabilities);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkGetPhysicalDeviceSurfaceCapabilitiesKHR failed. result=" << result;
                return details;
            }
            uint32_t formatCount;
            vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface_, &formatCount, nullptr);

            if (formatCount != 0) {
                details.formats.resize(formatCount);
                vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface_, &formatCount, details.formats.data());
            }

            uint32_t presentModeCount;
            vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface_, &presentModeCount, nullptr);

            if (presentModeCount != 0) {
                details.presentModes.resize(presentModeCount);
                vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface_, &presentModeCount, details.presentModes.data());
            }
            return details;
        }

        VkSurfaceFormatKHR ChooseSwapSurfaceFormat(const std::vector<VkSurfaceFormatKHR>& availableFormats)
        {
            for (const auto& availableFormat : availableFormats)
            {
                if (availableFormat.format == VK_FORMAT_B8G8R8A8_SRGB && availableFormat.colorSpace == VK_COLOR_SPACE_SRGB_NONLINEAR_KHR)
                {
                    return availableFormat;
                }
            }
            return availableFormats[0];
        }

        VkPresentModeKHR ChooseSwapPresentMode(const std::vector<VkPresentModeKHR>& availablePresentModes)
        {
            for (const auto& availablePresentMode : availablePresentModes)
            {
                if (availablePresentMode == VK_PRESENT_MODE_MAILBOX_KHR)
                    return availablePresentMode;
            }
            return VK_PRESENT_MODE_FIFO_KHR;
        }

        VkExtent2D ChooseSwapExtent(const VkSurfaceCapabilitiesKHR& capabilities)
        {
            if (capabilities.currentExtent.width != std::numeric_limits<uint32_t>::max())
                return capabilities.currentExtent;
            RTC_CHECK_NOTREACHED();

//            int width, height;
//            glfwGetFramebufferSize(window, &width, &height);
//
//            VkExtent2D actualExtent = {
//                static_cast<uint32_t>(width),
//                static_cast<uint32_t>(height)
//            };
//
//            actualExtent.width = std::clamp(actualExtent.width, capabilities.minImageExtent.width, capabilities.maxImageExtent.width);
//            actualExtent.height = std::clamp(actualExtent.height, capabilities.minImageExtent.height, capabilities.maxImageExtent.height);
//
//            return actualExtent;
        }

        VkResult RecordCommandBuffer(const NativeFrameBuffer* buffer, VkCommandBuffer commandBuffer, uint32_t imageIndex)
        {
            // begin recording command.
            VkCommandBufferBeginInfo beginInfo = {};
            beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;

            VkResult result = vkBeginCommandBuffer(commandBuffer, &beginInfo);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkBeginCommandBuffer failed. result=" << result;
                return result;
            }

            // begin a render pass.
            VkRenderPassBeginInfo renderPassInfo = {};
            renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
            renderPassInfo.renderPass = renderPass_;
            renderPassInfo.framebuffer = framebuffers_[imageIndex];
            renderPassInfo.renderArea.offset = {0, 0};
            renderPassInfo.renderArea.extent = swapchainExtent_;
            VkClearValue clearColor = {{{0.0f, 0.0f, 0.0f, 1.0f}}};
            renderPassInfo.clearValueCount = 1;
            renderPassInfo.pClearValues = &clearColor;
            vkCmdBeginRenderPass(commandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);

            // todo: do something.
            // vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, graphicsPipeline);

            // end a render pass.
            vkCmdEndRenderPass(commandBuffer);

            // end recording command.
            result = vkEndCommandBuffer(commandBuffer);
            if(result != VK_SUCCESS)
            {
                RTC_LOG(LS_INFO) << "vkEndCommandBuffer failed. result=" << result;
                return result;
            }
            return VK_SUCCESS;
        }
    };

    class EGLSurface : public Surface
    {
    public:
        EGLSurface() {}
        ~EGLSurface() {}

        void DrawFrame(const NativeFrameBuffer* buffer) override
        {
        }
    };

    std::unique_ptr<Surface> CreateVulkanSurface(VkSurfaceKHR surface, VkDevice device, VkPhysicalDevice physicalDevice)
    {
        return std::make_unique<VulkanSurface>(surface, device, physicalDevice);
    }

    std::unique_ptr<Surface> CreateEGLSurface()
    {
        return std::make_unique<EGLSurface>();
    }


}
}